class BarterApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.token = null;
    }

    async login(userId, role) {
        const response = await fetch(`${this.baseUrl}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, role })
        });
        
        if (!response.ok) throw new Error('Помилка сервера при вході');
        
        const data = await response.json();
        this.token = data.token;
        return data;
    }

    async getOffers() {
        if (!this.token) throw new Error('Спочатку увійдіть!');

        const response = await fetch(`${this.baseUrl}/api/offers`, {
            headers: { 'Authorization': `Bearer ${this.token}` }
        });
        
        if (!response.ok) throw new Error('Помилка завантаження оголошень');
        return await response.json();
    }

    async createOffer(data) {
        if (!this.token) throw new Error('Спочатку увійдіть!');

        const response = await fetch(`${this.baseUrl}/api/offers`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.token}`
            },
            body: JSON.stringify(data)
        });
        
        if (response.status === 201 || response.status === 200) {
            return { ok: true, status: response.status, data: await response.json() };
        } else {
            return { ok: false, status: response.status, text: await response.text() };
        }
    }
}

// Створюємо екземпляр для використання на сторінці
const apiClient = new BarterApiClient('');