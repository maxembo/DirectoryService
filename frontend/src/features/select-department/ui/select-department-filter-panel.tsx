import { FilterSelect } from "@/shared/components/filter-select";
import {
	activeFilterOptions,
	sortDirectionOptions,
} from "@/shared/model/filter-options";
import type { FilterOption } from "@/shared/model/filter-types";
import {
	setDepartmentIsActive,
	setDepartmentIsParent,
	setDepartmentSortBy,
	setDepartmentSortDirection,
	useDepartmentIsActive,
	useDepartmentIsParent,
	useDepartmentSortBy,
	useDepartmentSortDirection,
	type DepartmentListId,
	type DepartmentParentFilter,
	type DepartmentSortByFilter,
} from "@/entities/departments";

const departmentSortByOptions: Array<FilterOption<DepartmentSortByFilter>> = [
	{ value: "name", label: "По имени" },
	{ value: "path", label: "По пути" },
	{ value: "created", label: "По созданию" },
];

const departmentParentOptions: Array<FilterOption<DepartmentParentFilter>> = [
	{ value: "all", label: "Все" },
	{ value: "parent", label: "Родители" },
	{ value: "children", label: "Дочерние" },
];

type Props = {
	stateId?: DepartmentListId;
	hideStatusFilter?: boolean;
};

export function SelectDepartmentFilterPanel({
	stateId,
	hideStatusFilter = false,
}: Props) {
	const isParent = useDepartmentIsParent(stateId);
	const isActive = useDepartmentIsActive(stateId);
	const sortBy = useDepartmentSortBy(stateId);
	const sortDirection = useDepartmentSortDirection(stateId);

	return (
		<aside className="bg-muted/20 h-fit space-y-2 rounded-2xl border p-5">
			{!hideStatusFilter && (
				<FilterSelect
					value={isActive}
					label="Статус"
					onValueChange={(value) => setDepartmentIsActive(value, stateId)}
					items={activeFilterOptions}
				/>
			)}

			<FilterSelect
				value={isParent}
				label="Родительский отдел"
				onValueChange={(value) => setDepartmentIsParent(value, stateId)}
				items={departmentParentOptions}
			/>

			<FilterSelect
				value={sortBy}
				label="Сортировка"
				onValueChange={(value) => setDepartmentSortBy(value, stateId)}
				items={departmentSortByOptions}
			/>

			<FilterSelect
				value={sortDirection}
				label="Направление"
				onValueChange={(value) => setDepartmentSortDirection(value, stateId)}
				items={sortDirectionOptions}
			/>
		</aside>
	);
}
