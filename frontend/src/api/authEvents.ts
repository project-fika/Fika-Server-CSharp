type UnauthenticatedCallback = () => void;

class AuthEventManager {
    private listeners: UnauthenticatedCallback[] = [];

    public onUnauthenticated(callback: UnauthenticatedCallback) {
        this.listeners.push(callback);
        return () => {
            this.listeners = this.listeners.filter((l) => l !== callback);
        };
    }

    public emitUnauthenticated() {
        for (const callback of this.listeners) {
            callback();
        }
    }
}

export const authEvents = new AuthEventManager();
