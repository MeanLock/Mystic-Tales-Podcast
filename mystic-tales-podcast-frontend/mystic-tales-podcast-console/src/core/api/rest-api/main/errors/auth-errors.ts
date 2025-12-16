export class LoginRequiredError extends Error {
    constructor(message: string) {
        super(message);
        this.name = "LoginRequiredError";
    }
}

export class TokenExpiredError extends Error {
    constructor(message : string) {
        super(message);
        this.name = "TokenExpiredError";
    }
}
