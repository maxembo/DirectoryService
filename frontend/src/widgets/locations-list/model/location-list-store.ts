import type { DepartmentId, DepartmentShortDto } from "@/entities/departments";
import type { LocationSortByFilter } from "@/entities/locations";
import { PAGE_SIZE } from "@/shared/api/pagination-request";
import { SortDirectionFilter } from "@/shared/model/filter-types";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";
import { useShallow } from "zustand/shallow";

export type LocationListId = string;

type LocationListState = {
	selectedDepartments: DepartmentShortDto[];
	search: string;
	sortBy: LocationSortByFilter;
	sortDirection: SortDirectionFilter;
	pageSize: number;
};

type LocationListStates = Record<LocationListId, LocationListState | undefined>;

const DEFAULT_STATE_ID = "__default__";

const initialState: LocationListState = {
	selectedDepartments: [],
	search: "",
	sortBy: "name",
	sortDirection: "asc",
	pageSize: PAGE_SIZE,
};

const initialStates: LocationListStates = {};

const resolveStateId = (stateId?: LocationListId) =>
	stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (
	states: LocationListStates,
	stateId?: LocationListId,
): LocationListState => {
	const id = resolveStateId(stateId);

	if (!states[id]) states[id] = { ...initialState };

	return states[id];
};

const useLocationListStore = create<LocationListStates>()(
	persist(() => ({ ...initialStates }), {
		name: "location-list-storage",
		storage: createJSONStorage(() => localStorage),
		partialize: (state) =>
			Object.fromEntries(
				Object.entries(state).filter(([key]) => key === DEFAULT_STATE_ID),
			),
	}),
);

export const useLocationSelectedDepartments = (stateId?: LocationListId) =>
	useLocationListStore(
		(states) => getOrCreate(states, stateId).selectedDepartments,
	);

export const setLocationSelectedDepartments = (
	selectedDepartments: DepartmentShortDto[],
	stateId?: LocationListId,
) =>
	useLocationListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			selectedDepartments,
		},
	}));

export const removeLocationSelectedDepartments = (
	id: DepartmentId,
	stateId?: LocationListId,
) =>
	useLocationListStore.setState((states) => {
		const state = getOrCreate(states, stateId);
		return {
			[resolveStateId(stateId)]: {
				...state,
				selectedDepartments: state.selectedDepartments.filter(
					(dep) => dep.id !== id,
				),
			},
		};
	});

export const useLocationList = (stateId?: LocationListId) => {
	return useLocationListStore(
		useShallow((states) => getOrCreate(states, stateId)),
	);
};

export const useLocationSearch = (stateId?: LocationListId) =>
	useLocationListStore((states) => getOrCreate(states, stateId).search);

export const setLocationSearch = (search: string, stateId?: LocationListId) => {
	useLocationListStore.setState((states) => ({
		[resolveStateId(stateId)]: { ...getOrCreate(states, stateId), search },
	}));
};

export const useLocationSortBy = (stateId?: LocationListId) =>
	useLocationListStore((states) => getOrCreate(states, stateId).sortBy);

export const setLocationSortBy = (
	sortBy: LocationSortByFilter,
	stateId?: LocationListId,
) =>
	useLocationListStore.setState((states) => ({
		[resolveStateId(stateId)]: { ...getOrCreate(states, stateId), sortBy },
	}));

export const useLocationSortDirection = (stateId?: LocationListId) =>
	useLocationListStore((states) => getOrCreate(states, stateId).sortDirection);

export const setLocationSortDirection = (
	sortDirection: SortDirectionFilter,
	stateId?: LocationListId,
) =>
	useLocationListStore.setState((states) => ({
		[resolveStateId(stateId)]: {
			...getOrCreate(states, stateId),
			sortDirection,
		},
	}));
