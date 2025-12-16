export enum AppErrorType {
    LoginRequired = "LoginRequiredError",
    TokenExpired = "TokenExpiredError",
    Unauthorized = "UnauthorizedError",
    Forbidden = "ForbiddenError",
    Network = "NetworkError",
    GraphQL = "GraphQLError",
    Unknown = "UnknownError",
}

export class AppError extends Error {
    public type: AppErrorType;

    constructor(type: AppErrorType, message: string) {
        super(message);
        this.name = type;
        this.type = type;
    }
}
