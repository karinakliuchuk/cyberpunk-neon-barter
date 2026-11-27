class BarterApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.token = localStorage.getItem('jwt_token') || null; 
    }

    async register(username, password) {
        const response = await fetch(`${this.baseUrl}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            // ВИПРАВЛЕНО: тепер надсилаємо callsign
            body: JSON.stringify({ callsign: username, password: password })
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Помилка реєстрації');
        }
        return await response.json();
    }

    async login(username, password) {
        const response = await fetch(`${this.baseUrl}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            // ВИПРАВЛЕНО: тепер надсилаємо callsign
            body: JSON.stringify({ callsign: username, password: password })
        });
        
        if (!response.ok) throw new Error('Невірний логін або пароль');
        
        const data = await response.json();
        this.token = data.token;
        localStorage.setItem('jwt_token', this.token);
        return data;
    }

    logout() {
        this.token = null;
        localStorage.removeItem('jwt_token');
    }

    async getOffers() {
        if (!this.token) throw new Error('Спочатку увійдіть!');
        const response = await fetch(`${this.baseUrl}/api/offers`, {
            headers: { 'Authorization': `Bearer ${this.token}` }
        });
        if (!response.ok) throw new Error('Помилка завантаження оголошень');
        return await response.json();
    }

    async getMyOffers() {
        if (!this.token) throw new Error('Спочатку увійдіть!');
        const response = await fetch(`${this.baseUrl}/api/offers/my`, {
            headers: { 'Authorization': `Bearer ${this.token}` }
        });
        if (!response.ok) throw new Error('Помилка завантаження моїх оголошень');
        return await response.json();
    }

    async findCycle() {
        if (!this.token) throw new Error('Спочатку увійдіть!');
        const response = await fetch(`${this.baseUrl}/api/cycles/find`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${this.token}` }
        });
        
        if (!response.ok) {
            if (response.status === 409) throw new Error('Цикл обміну не знайдено (потрібно більше оголошень).');
            throw new Error('Помилка пошуку циклу');
        }
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

const apiClient = new BarterApiClient('');