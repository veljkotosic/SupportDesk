FROM postgres:18

COPY 01-init-replication.sh /docker-entrypoint-initdb.d/01-init-replication.sh
COPY entrypoint-primary.sh /usr/local/bin/entrypoint-primary.sh
COPY entrypoint-replica.sh /usr/local/bin/entrypoint-replica.sh

RUN chmod +x /docker-entrypoint-initdb.d/01-init-replication.sh \
    && chmod +x /usr/local/bin/entrypoint-primary.sh \
    && chmod +x /usr/local/bin/entrypoint-replica.sh

ENTRYPOINT ["entrypoint-primary.sh"]
