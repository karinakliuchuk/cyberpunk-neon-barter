// NFR-01: пошук циклів ≤ 5 с на 1000 оголошень
// УВАГА: POST /api/cycles/find СТВОРЮЄ угоду (змінює дані). Перед кожним прогоном база
// має бути заповнена 1000 оголошень (seed), інакше після перших викликів отримаємо швидкі 409
// і вимірювання буде безглуздим. Seed-скрипт буде додано після перегляду Domain/Entities.
//
// Запуск: k6 run -e BASE_URL=http://localhost:5254 Tests/Performance/nfr01-search.js
import http from 'k6/http';
import { check } from 'k6';

const BASE = __ENV.BASE_URL || 'http://localhost:5254';

export const options = {
  vus: 1,
  iterations: 5,
  thresholds: {
    http_req_duration: ['p(95)<5000'],   // NFR-01
  },
};

export function setup() {
  const r = http.post(`${BASE}/api/auth/login`,
    JSON.stringify({ userId: '00000000-0000-0000-0000-000000000001', role: 'Participant' }),
    { headers: { 'Content-Type': 'application/json' } });
  return { token: r.json('token') };
}

export default function (data) {
  const r = http.post(`${BASE}/api/cycles/find`, null,
    { headers: { Authorization: `Bearer ${data.token}` } });
  check(r, { 'status 200 або 409': (x) => x.status === 200 || x.status === 409 });
}
