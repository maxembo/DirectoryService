export type ApiError = {
	code: string;
	message: string;
	type: ErrorType;
	invalidField?: string | null;
};

const errorMessage = "Неизвестная ошибка";

export type ErrorType = "validation" | "not_found" | "failure" | "conflict";

export class EnvelopeError extends Error {
	public readonly errorsList: ApiError[];

	constructor(errorsList: ApiError[]) {
		const error = errorsList[0].message;

		super(error);

		this.name = "EnvelopeError";
		this.errorsList = errorsList;

		Object.setPrototypeOf(this, EnvelopeError.prototype);
	}

	get firstMessage(): string {
		return this.errorsList[0].message ?? errorMessage;
	}

	get allMessages() {
		return this.errorsList.map((msg) => msg.message).join(", ");
	}
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
	return error instanceof EnvelopeError;
}
