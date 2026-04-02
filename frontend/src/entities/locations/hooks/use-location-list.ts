import { locationsApi } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";
import { SortByFilter, SortDirectionFilter } from "./use-location-filters";

export type GetLocationsParams = {
	search?: string;
	departmentIds?: string[];
	isActive?: boolean;
	sortBy?: SortByFilter;
	sortDirection?: SortDirectionFilter;
	page: number;
	pageSize: number;
};

export function useLocationList(params: GetLocationsParams) {
	const { data, isLoading, isFetching, error } = useQuery({
		...locationsApi.getLocationsQueryOptions({
			page: params.page,
			search: params.search,
			isActive: params.isActive,
			sortBy: params.sortBy,
			sortDirection: params.sortDirection,
			pageSize: params.pageSize,
			departmentIds: params.departmentIds,
		}),
	});

	return {
		locations: data?.result?.items || [],
		totalPages: data?.result?.totalPages || 0,
		isLoading,
		isFetching,
		error,
	};
}
