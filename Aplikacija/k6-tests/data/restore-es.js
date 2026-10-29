#!/usr/bin/env node

/**
 * SupportDesk Elasticsearch Restore Script
 * 
 * Recreates Elasticsearch indices with production mappings and
 * bulk-loads data from 'data/seed-es.ndjson' in streaming chunks.
 * 
 * Usage:
 *   bun run scripts/restore-es.js [http://localhost:9200]
 *   node scripts/restore-es.js [http://localhost:9200]
 */

import fs from 'node:fs';
import path from 'node:path';
import readline from 'node:readline';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const ES_URL = process.argv[2] || process.env.ELASTICSEARCH_URL || 'http://localhost:9200';
const DATA_DIR = fs.existsSync(path.join(__dirname, 'seed-es.ndjson')) 
  ? __dirname 
  : path.resolve(__dirname, '..', 'data');
const DUMP_FILE = path.join(DATA_DIR, 'seed-es.ndjson');

const BATCH_SIZE = 5000; // 5000 docs = 10000 lines per bulk request (~4 MB)

console.log('==========================================================');
console.log('   SUPPORTDESK ELASTICSEARCH RESTORE RUNNER               ');
console.log('==========================================================');
console.log(` Target ES Host : ${ES_URL}`);
console.log(` Dump File      : ${DUMP_FILE}\n`);

if (!fs.existsSync(DUMP_FILE)) {
  console.error(`[!] Error: Dump file '${DUMP_FILE}' not found!`);
  process.exit(1);
}

const INDICES = {
  organizations: {
    mappings: {
      properties: {
        id: { type: 'keyword' },
        name: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        }
      }
    }
  },
  users: {
    mappings: {
      properties: {
        id: { type: 'keyword' },
        organizationId: { type: 'keyword' },
        userName: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        email: { type: 'keyword' },
        role: { type: 'short' }
      }
    }
  },
  tickets: {
    mappings: {
      properties: {
        id: { type: 'keyword' },
        organizationId: { type: 'keyword' },
        organizationName: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        categoryId: { type: 'keyword' },
        categoryName: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        customerId: { type: 'keyword' },
        customerUserName: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        customerEmail: { type: 'keyword' },
        supportAgentId: { type: 'keyword' },
        supportAgentUserName: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        status: { type: 'short' },
        priority: { type: 'short' },
        feedback: { type: 'short' },
        subject: {
          type: 'text',
          fields: { keyword: { type: 'keyword' } }
        },
        openedAt: { type: 'date' },
        assignedAt: { type: 'date' },
        closedAt: { type: 'date' },
        lastMessageAt: { type: 'date' },
        unreadNotifications: {
          type: 'nested',
          properties: {
            id: { type: 'keyword' },
            organizationId: { type: 'keyword' },
            ticketId: { type: 'keyword' },
            text: { type: 'text' },
            status: { type: 'short' },
            createdAt: { type: 'date' }
          }
        }
      }
    }
  }
};

async function main() {
  const startTime = Date.now();

  // 1. Delete and recreate indices with schema mappings
  console.log('[*] Resetting indices with schema mappings...');
  await fetch(`${ES_URL}/dashboards`, { method: 'DELETE' }).catch(() => {});
  for (const [indexName, schema] of Object.entries(INDICES)) {
    await fetch(`${ES_URL}/${indexName}`, { method: 'DELETE' }).catch(() => {});
    const res = await fetch(`${ES_URL}/${indexName}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(schema)
    });
    if (!res.ok && res.status !== 400) {
      const err = await res.text();
      console.error(`[!] Failed creating index '${indexName}': ${err}`);
      process.exit(1);
    }
  }
  console.log('[✔] Indices created with production mappings.\n');

  // 2. Stream ndjson file in chunks to _bulk
  console.log('[*] Streaming seed-es.ndjson to Elasticsearch...');
  const fileStream = fs.createReadStream(DUMP_FILE, { encoding: 'utf8' });
  const rl = readline.createInterface({ input: fileStream, crlfDelay: Infinity });

  let batchLines = [];
  let docCount = 0;
  let totalDocs = 0;

  for await (const line of rl) {
    if (!line.trim()) continue;
    batchLines.push(line);
    docCount++;

    if (docCount >= BATCH_SIZE * 2) {
      await sendBulk(batchLines.join('\n') + '\n');
      totalDocs += BATCH_SIZE;
      if (totalDocs % 50000 === 0) {
        console.log(`    -> Restored ${totalDocs.toLocaleString()} documents...`);
      }
      batchLines = [];
      docCount = 0;
    }
  }

  if (batchLines.length > 0) {
    await sendBulk(batchLines.join('\n') + '\n');
    totalDocs += docCount / 2;
  }

  // 3. Refresh indices
  console.log('\n[*] Refreshing indices...');
  await fetch(`${ES_URL}/_refresh`, { method: 'POST' });

  const durationSec = ((Date.now() - startTime) / 1000).toFixed(1);
  console.log(`[✔] Restore completed in ${durationSec}s! Total documents restored: ${totalDocs.toLocaleString()}`);

  // 4. Verify counts
  for (const indexName of Object.keys(INDICES)) {
    const res = await fetch(`${ES_URL}/${indexName}/_count`);
    const data = await res.json();
    console.log(`    - ${indexName.padEnd(15)}: ${(data.count || 0).toLocaleString()} documents`);
  }

  console.log('\n[✔] ELASTICSEARCH IS READY FOR TESTING!');
}

async function sendBulk(bulkPayload) {
  const res = await fetch(`${ES_URL}/_bulk`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-ndjson' },
    body: bulkPayload
  });
  if (!res.ok) {
    const err = await res.text();
    throw new Error(`Bulk request failed HTTP ${res.status}: ${err.slice(0, 500)}`);
  }
  const result = await res.json();
  if (result.errors) {
    const firstErr = result.items.find(i => i.index && i.index.error);
    console.warn(`[!] Warning: Some documents had errors: ${JSON.stringify(firstErr?.index?.error)}`);
  }
}

main().catch(err => {
  console.error('[!] Fatal error during restore:', err);
  process.exit(1);
});
