import { ApiError } from "./errors";

export type Envelope<T = unknown> = {
	result: T | null;
	errorsList: ApiError[];
	isError: boolean;
	timeGenerated: string;
};

export type PaginationEnvelope<T> = {
	items: T[];
	totalCount: number;
	page: number;
	pageSize: number;
	totalPages: number;
};
