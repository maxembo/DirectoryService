import { locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/errors";
import { SortByFilter, SortDirectionFilter } from "@/shared/api/filters";
import { useQuery } from "@tanstack/react-query";

type GetLocationsParams = {
	search?: string;
	departmentIds?: string[];
	isActive?: boolean;
	sortBy?: SortByFilter;
	sortDirection?: SortDirectionFilter;
	page: number;
	pageSize: number;
};

export function useLocationQuery(params: GetLocationsParams) {
	const query = useQuery(locationsApi.getLocationsQueryOptions(params));

	return {
		locations: query.data?.result?.items || [],
		totalPages: query.data?.result?.totalPages || 0,
		isPending: query.isPending,
		isError: query.isError,
		error: query.error instanceof EnvelopeError ? query.error : undefined,
	};
}
