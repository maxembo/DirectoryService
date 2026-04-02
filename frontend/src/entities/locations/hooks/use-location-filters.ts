import {
	ActiveFilter,
	SortByFilter,
	SortDirectionFilter,
} from "@/shared/api/filters";
import { PAGE_SIZE } from "@/shared/api/pagination-request";
import { useState } from "react";

type LocationListState = {
	departmentIds: string[];
	search: string;
	isActive: ActiveFilter;
	sortBy: SortByFilter;
	sortDirection: SortDirectionFilter;
	page: number;
	pageSize: number;
};

const initialState: LocationListState = {
	departmentIds: [],
	search: "",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
	page: 1,
	pageSize: PAGE_SIZE,
};

export function useLocationFilters() {
	const [state, setState] = useState<LocationListState>(initialState);

	const setPage = (page: number) => setState((prev) => ({ ...prev, page }));
	const setSearch = (search: string) =>
		setState((prev) => ({ ...prev, page: 1, search }));

	const setIsActive = (isActive: ActiveFilter) =>
		setState((prev) => ({ ...prev, page: 1, isActive }));

	const departmentIds = (departmentIds: string[]) =>
		setState((prev) => ({ ...prev, page: 1, departmentIds }));

	const setSortBy = (sortBy: SortByFilter) =>
		setState((prev) => ({ ...prev, page: 1, sortBy }));

	const setSortDirection = (sortDirection: SortDirectionFilter) =>
		setState((prev) => ({ ...prev, page: 1, sortDirection }));

	return {
		state,
		setPage,
		setSearch,
		setIsActive,
		departmentIds,
		setSortBy,
		setSortDirection,
	};
}
