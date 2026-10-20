import { sleep } from 'k6';

export function waitBetween(minSeconds, maxSeconds) {
    const seconds = minSeconds + Math.random() * (maxSeconds - minSeconds);
    sleep(seconds);
}