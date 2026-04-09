import {
	SortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import { useState } from "react";
import { ActiveFilter } from "./types";

type LocationListState = {
	departmentIds: string[];
	search: string;
	isActive: ActiveFilter;
	sortBy: SortByFilter;
	sortDirection: SortDirectionFilter;
	page: number;
};

const initialLocationFiltersState: LocationListState = {
	departmentIds: [],
	search: "",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
	page: 1,
};

export function useLocationFilters() {
	const [filters, setFilters] = useState<LocationListState>(
		initialLocationFiltersState,
	);

	const update = (patch: Partial<LocationListState>) =>
		setFilters((prev) => ({ ...prev, ...patch }));

	const setPage = (page: number) => update({ page });

	const setSearch = (search: string) => update({ search, page: 1 });

	const setIsActive = (isActive: ActiveFilter) => update({ isActive, page: 1 });

	const setDepartmentIds = (departmentIds: string[]) =>
		update({ departmentIds, page: 1 });

	const setSortBy = (sortBy: SortByFilter) => update({ sortBy, page: 1 });

	const setSortDirection = (sortDirection: SortDirectionFilter) =>
		update({ sortDirection, page: 1 });

	return {
		filters,
		setPage,
		setSearch,
		setIsActive,
		setDepartmentIds,
		setSortBy,
		setSortDirection,
	};
}
