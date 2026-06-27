import {
	ActiveFilter,
	FilterOption,
	SortDirectionFilter,
} from "./filter-types";

export const activeFilterOptions: Array<FilterOption<ActiveFilter>> = [
	{ value: "all", label: "Все" },
	{ value: "active", label: "Активные" },
	{ value: "inactive", label: "Неактивные" },
];

export const sortDirectionOptions: Array<FilterOption<SortDirectionFilter>> = [
	{ value: "asc", label: "По возрастанию" },
	{ value: "desc", label: "По убыванию" },
];
