"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/shared/ui/list-empty";
import { ListError } from "@/shared/ui/list-error";
import { DepartmentPositionCard } from "./department-position-card";
import { PositionFilters } from "@/features/positions";
import { useDepartmentPositionsList } from "../../model/use-department-positions-list";
import type { DepartmentId } from "@/entities/departments";

const POSITIONS_DEPARTMENT_SELECT_STATE_ID = "positions-select-department";

type Props = {
	positionsDepartmentId: DepartmentId | null;
};

export function DepartmentPositionsList({ positionsDepartmentId }: Props) {
	const {
		positions,
		isFetchingNextPage,
		cursorRef,
		isError,
		error,
		isPending,
		refetch,
	} = useDepartmentPositionsList({
		stateId: POSITIONS_DEPARTMENT_SELECT_STATE_ID,
		positionsDepartmentId,
	});

	return (
		<div className="flex h-full min-h-0 flex-col space-y-4">
			<div className="shrink-0 space-y-3">
				<h1 className="text-2xl font-bold tracking-tight">Позиции</h1>
				<PositionFilters stateId={POSITIONS_DEPARTMENT_SELECT_STATE_ID} />
			</div>

			{!positionsDepartmentId ? (
				<ListEmpty title="Выберите подразделение" />
			) : isPending ? (
				<div className="flex min-h-0 flex-1 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<ListError
					message={error?.message ?? "Неизвестная ошибка"}
					onRetry={refetch}
				/>
			) : positions.length === 0 ? (
				<ListEmpty title="Позиции не найдены" />
			) : (
				<div className="bg-background/40 min-h-0 flex-1 overflow-y-auto overscroll-contain rounded-md border">
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
