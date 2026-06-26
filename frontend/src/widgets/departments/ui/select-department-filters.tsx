import {
	DepartmentParentFilter,
	DepartmentSortByFilter,
} from "@/entities/departments/model/types";
import {
	DepartmentListId,
	setDepartmentIsActive,
	setDepartmentIsParent,
	setDepartmentSearch,
	setDepartmentSortBy,
	setDepartmentSortDirection,
	useDepartmentIsActive,
	useDepartmentIsParent,
	useDepartmentSearch,
	useDepartmentSortBy,
	useDepartmentSortDirection,
} from "@/features/departments/model/department-list-store";
import { Input } from "@/shared/components/ui/input";
import {
	activeItems,
	FilterSelect,
	sortDirectionItems,
} from "@/widgets/locations-list/ui/location-filters";
import { Search } from "lucide-react";

const sortByItems: Array<{ value: DepartmentSortByFilter; label: string }> = [
	{
		value: "name",
		label: "По имени",
	},
	{ value: "path", label: "По пути" },
	{ value: "created", label: "По создании" },
];

const parentIdItems: Array<{ value: DepartmentParentFilter; label: string }> = [
	{ value: "all", label: "Все" },
	{ value: "parent", label: "Родители" },
	{ value: "children", label: "Дочерние" },
];

export function SelectDepartmentSearch({
	stateId,
}: {
	stateId?: DepartmentListId;
}) {
	const search = useDepartmentSearch(stateId);

	return (
		<div className="relative">
			<Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />

			<Input
				placeholder="Поиск по названию"
				className="h-12 pl-11"
				value={search ?? ""}
				onChange={(event) => setDepartmentSearch(event.target.value, stateId)}
			/>
		</div>
	);
}

export function SelectDepartmentFilterPanel({
	stateId,
}: {
	stateId?: DepartmentListId;
}) {
	const isParent = useDepartmentIsParent(stateId);
	const isActive = useDepartmentIsActive(stateId);
	const sortBy = useDepartmentSortBy(stateId);
	const sortDirection = useDepartmentSortDirection(stateId);

	return (
		<aside className="space-y-2 rounded-2xl border bg-muted/20 p-5">
			<FilterSelect
				value={isActive}
				label="Статус"
				onValueChange={(value) => setDepartmentIsActive(value, stateId)}
				items={activeItems}
			/>

			<FilterSelect
				value={isParent}
				label="Родительский отдел"
				onValueChange={(value) => setDepartmentIsParent(value, stateId)}
				items={parentIdItems}
			/>

			<FilterSelect
				value={sortBy}
				label="Сортировка"
				onValueChange={(value) => setDepartmentSortBy(value, stateId)}
				items={sortByItems}
			/>

			<FilterSelect
				value={sortDirection}
				label="Направление"
				onValueChange={(value) => setDepartmentSortDirection(value, stateId)}
				items={sortDirectionItems}
			/>
		</aside>
	);
}
