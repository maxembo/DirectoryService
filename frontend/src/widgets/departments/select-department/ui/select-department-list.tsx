import {
	DepartmentId,
	DepartmentShortDto,
} from "@/entities/departments/model/types";
import { DepartmentListId } from "@/features/departments/model/department-list-store";
import { useInfiniteDepartmentsList } from "@/features/departments/model/use-infinite-departments-list";
import { SelectDepartmentCard } from "@/features/departments/select-department/ui/select-department-card";
import { SelectDepartmentFilterPanel } from "@/features/departments/select-department/ui/select-department-filter-panel";
import { SelectDepartmentSearch } from "@/features/departments/select-department/ui/select-department-filter-search";
import { SelectedDepartment } from "@/features/departments/select-department/ui/selected-department";
import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/widgets/list-empty";
import { ListError } from "@/widgets/list-error";
import { useMemo } from "react";

type Props = {
	stateId?: DepartmentListId;
	selectedDepartments: DepartmentShortDto[];
	onChange: (selectedDepartments: DepartmentShortDto[]) => void;
	multiSelect?: boolean;
	excludeIds?: string[];
};

export function SelectDepartmentList({
	stateId,
	selectedDepartments,
	onChange,
	multiSelect,
	excludeIds = [],
}: Props) {
	const {
		departments,
		error,
		cursorRef,
		isPending,
		isError,
		isFetchingNextPage,
	} = useInfiniteDepartmentsList({ stateId });

	const filteredDepartments = useMemo(() => {
		return departments.filter((dep) => !excludeIds.includes(dep.id));
	}, [departments, excludeIds]);

	const handleCheckedChange = (
		selected: boolean,
		department: DepartmentShortDto,
	) => {
		if (multiSelect) {
			if (selected) {
				onChange([...selectedDepartments, department]);
			} else {
				onChange(selectedDepartments.filter((dep) => dep.id !== department.id));
			}
			return;
		}

		if (selected) {
			onChange([department]);
		} else {
			onChange([]);
		}
	};

	const handleRemoveDepartment = (id: DepartmentId) => {
		onChange(selectedDepartments.filter((dep) => dep.id !== id));
	};

	const isSelected = (departmentId: DepartmentId) => {
		return selectedDepartments.some((dep) => dep.id === departmentId);
	};
	return (
		<div className="grid h-full min-h-0 gap-6 lg:grid-cols-[1fr_210px]">
			<div className="flex h-full min-h-0 flex-col gap-4">
				<div className="flex flex-col gap-4 shrink-0">
					<SelectDepartmentSearch stateId={stateId} />

					<SelectedDepartment
						selectedDepartments={selectedDepartments}
						onRemove={handleRemoveDepartment}
					/>
				</div>

				{isPending ? (
					<div className="flex min-h-60 items-center justify-center">
						<Spinner />
					</div>
				) : isError ? (
					<ListError message={error?.message ?? "Неизвестная ошибка"} />
				) : departments?.length === 0 ? (
					<ListEmpty title="Подразделение" />
				) : (
					<div className="min-h-0 flex-1 overflow-y-auto">
						<div className="flex flex-col gap-3 px-5 py-2">
							{filteredDepartments.map((department) => (
								<SelectDepartmentCard
									key={department.id}
									department={department}
									checked={isSelected(department.id)}
									onCheckedChange={handleCheckedChange}
								/>
							))}
						</div>

						<div ref={cursorRef} className="flex justify-center py-10">
							{isFetchingNextPage && <Spinner />}
						</div>
					</div>
				)}
			</div>

			<SelectDepartmentFilterPanel stateId={stateId} />
		</div>
	);
}
