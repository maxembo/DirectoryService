import { InfiniteData } from "@tanstack/react-query";
import { ApiError } from "./errors";

export type Envelope<T = unknown> = {
	result: T | null;
	errorsList: ApiError[];
	isError: boolean;
	timeGenerated: string;
};

export type PaginationEnvelope<T = unknown> = {
	items: T[];
	totalCount: number;
	page: number;
	pageSize: number;
	totalPages: number;
};

export const envelopeInfinityQueryOptions = <T>() => ({
	initialPageParam: 1,
	getNextPageParam: (response: Envelope<PaginationEnvelope<T>>) => {
		const result = response.result;

		if (!result || result.page >= result.totalPages) {
			return undefined;
		}
		return result.page + 1;
	},
	select: (
		data: InfiniteData<Envelope<PaginationEnvelope<T>>>,
	): Envelope<PaginationEnvelope<T>> => {
		const firstPage = data.pages[0];
		return {
			...firstPage,
			result: firstPage.result && {
				items: data.pages.flatMap((page) => page.result?.items ?? []),
				totalCount: firstPage.result.totalCount,
				totalPages: firstPage.result.totalPages,
				pageSize: firstPage.result.pageSize,
				page: firstPage.result.page,
			},
			isError: data.pages.some((page) => page.isError),
			errorsList: data.pages.flatMap((page) => page.errorsList || []),
			timeGenerated:
				data.pages.at(-1)?.timeGenerated || firstPage.timeGenerated,
		};
	},
});
