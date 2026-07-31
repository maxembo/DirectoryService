"use client";
import {
	DepartmentTreeId,
	useDepartmentTreeSelectedId,
} from "@/features/departments/department-tree/model/department-tree-store";
import { PositionFilters } from "@/features/positions/ui/position-filters";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentPositionsList } from "@/widgets/department-positions/model/use-department-positions-list";
import { ListEmpty } from "@/widgets/list-empty";
import { ListError } from "@/widgets/list-error";
import { DepartmentPositionCard } from "./department-position-card";

const POSITIONS_DEPARTMENT_SELECT_STATE_ID = "positions-select-department";

type Props = {
	departmentTreeStateId?: DepartmentTreeId;
};

export function DepartmentPositionsList({ departmentTreeStateId }: Props) {
	const {
		positions,
		isFetchingNextPage,
		cursorRef,
		isError,
		error,
		isPending,
	} = useDepartmentPositionsList({
		stateId: POSITIONS_DEPARTMENT_SELECT_STATE_ID,
		departmentTreeStateId,
	});

	const selectedDepartmentId = useDepartmentTreeSelectedId(
		departmentTreeStateId,
	);

	return (
		<div className="flex h-full min-h-0 flex-col space-y-4">
			<div className="shrink-0 space-y-3">
				<h1 className="text-2xl font-bold tracking-tight">Позиции</h1>
				<PositionFilters stateId={POSITIONS_DEPARTMENT_SELECT_STATE_ID} />
			</div>

			{!selectedDepartmentId ? (
				<ListEmpty title="Выберите подразделение" />
			) : isPending ? (
				<div className="flex min-h-0 flex-1 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<ListError message={error?.message ?? "Неизвестная ошибка"} />
			) : positions.length === 0 ? (
				<ListEmpty title="Позиции не найдены" />
			) : (
				<div className="min-h-0 flex-1 overflow-y-auto overscroll-contain rounded-md border bg-background/40">
					<div className="flex flex-col gap-3 p-2 pr-3">
						{positions.map((position) => (
							<DepartmentPositionCard key={position.id} position={position} />
						))}

						<div ref={cursorRef} className="flex justify-center py-6">
							{isFetchingNextPage && <Spinner />}
						</div>
					</div>
				</div>
			)}
		</div>
	);
}
