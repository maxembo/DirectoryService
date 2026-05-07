import {
	SortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import { PAGE_SIZE } from "@/shared/api/pagination-request";
import { ActiveFilter } from "@/widgets/model/types";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";
import { useShallow } from "zustand/shallow";

type LocationFiltersState = {
	departmentIds: string[];
	search: string;
	isActive: ActiveFilter;
	sortBy: SortByFilter;
	sortDirection: SortDirectionFilter;
	pageSize: number;
};

type Actions = {
	setSearch: (input: LocationFiltersState["search"]) => void;
	setIsActive: (isActive: LocationFiltersState["isActive"]) => void;
	setSortBy: (sortBy: LocationFiltersState["sortBy"]) => void;
	setSortDirection: (
		sortDirection: LocationFiltersState["sortDirection"],
	) => void;
	setDepartmentIds: (
		departmentIds: LocationFiltersState["departmentIds"],
	) => void;
};

const initialFilters: LocationFiltersState = {
	departmentIds: [],
	search: "",
	isActive: "all",
	sortBy: "name",
	sortDirection: "asc",
	pageSize: PAGE_SIZE,
};

type LocationFiltersStore = LocationFiltersState & Actions;

const useLocationsFilterStore = create<LocationFiltersStore>()(
	persist(
		(set) => ({
			...initialFilters,
			setSearch: (input: LocationFiltersState["search"]) =>
				set(() => ({ search: input.trim() })),
			setIsActive: (isActive: LocationFiltersState["isActive"]) =>
				set(() => ({ isActive })),
			setSortBy: (sortBy: LocationFiltersState["sortBy"]) =>
				set(() => ({ sortBy })),
			setSortDirection: (
				sortDirection: LocationFiltersState["sortDirection"],
			) => set(() => ({ sortDirection })),
			setDepartmentIds: (
				departmentIds: LocationFiltersState["departmentIds"],
			) => set(() => ({ departmentIds })),
		}),
		{
			name: "location-filter-storage",
			storage: createJSONStorage(() => localStorage),
		},
	),
);

export const useGetLocationFilters = () => {
	return useLocationsFilterStore(useShallow((state) => ({ ...state })));
};

export const setFilterSearch = (input: LocationFiltersState["search"]) => {
	useLocationsFilterStore.getState().setSearch(input);
};

export const setFilterIsActive = (
	isActive: LocationFiltersState["isActive"],
) => {
	useLocationsFilterStore.getState().setIsActive(isActive);
};

export const setFilterSortBy = (sortBy: LocationFiltersState["sortBy"]) => {
	useLocationsFilterStore.getState().setSortBy(sortBy);
};

export const setFilterSortDirection = (
	sortDirection: LocationFiltersState["sortDirection"],
) => {
	useLocationsFilterStore.getState().setSortDirection(sortDirection);
};

export const setFilterDepartmentIds = (
	departmentIds: LocationFiltersState["departmentIds"],
) => {
	useLocationsFilterStore.getState().setDepartmentIds(departmentIds);
};
