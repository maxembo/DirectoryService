import {
	SortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import { useState } from "react";
import { ActiveFilter } from "./types";

type LocationFilters = {
	departmentIds: string[];
	search: string;
	isActive: ActiveFilter;
	sortBy: SortByFilter;
	sortDirection: SortDirectionFilter;
	page: number;
};

const initialFilters: LocationFilters = {
	departmentIds: [],
	search: "",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
	page: 1,
};

export function useLocationFilters() {
	const [filters, setFilters] = useState<LocationFilters>(initialFilters);

	const setPage = (page: number) => {
		setFilters((prev) => ({ ...prev, page }));
	};

	const updateFilters = (patch: Partial<Omit<LocationFilters, "page">>) => {
		setFilters((prev) => ({
			...prev,
			...patch,
			page: 1,
		}));
	};

	const setSearch = (search: string) => updateFilters({ search });

	const setIsActive = (isActive: ActiveFilter) => updateFilters({ isActive });

	const setDepartmentIds = (departmentIds: string[]) =>
		updateFilters({ departmentIds });

	const setSortBy = (sortBy: SortByFilter) => updateFilters({ sortBy });

	const setSortDirection = (sortDirection: SortDirectionFilter) =>
		updateFilters({ sortDirection });

	return {
		filters,
		actions: {
			setPage,
			setSearch,
			setIsActive,
			setDepartmentIds,
			setSortBy,
			setSortDirection,
		},
	};
}
