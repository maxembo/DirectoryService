"use client";

import { useArchiveView, useFocusedCard } from "@/shared/hooks";

import { DepartmentViewSwitch } from "./department-view-switch";
import { FocusedDepartment } from "./focused-department/focused-department";
import { DepartmentPositions } from "./department-positions/department-positions";
import { ArchivedDepartmentList } from "./archive/archived-department-list";
import {
	setDepartmentTreeSelectedId,
	useDepartmentTreeSelectedId,
} from "@/features/department-tree";
import { useEffect } from "react";

export function DepartmentView() {
	const { view, setView } = useArchiveView();
	const selectedId = useDepartmentTreeSelectedId();

	const { focusedId, handleRemoveFocus } = useFocusedCard();

	const positionsDepartmentId =
		view === "active" ? (selectedId ?? focusedId) : null;

	const isSelected =
		view === "active" && focusedId !== null && selectedId === null;

	useEffect(() => {
		if (focusedId) {
			setDepartmentTreeSelectedId(null);
		}
	}, [focusedId]);

	const focusedDepartment = focusedId ? (
		<FocusedDepartment
			key={focusedId}
			id={focusedId}
			onRemove={handleRemoveFocus}
			view={view}
			isSelected={isSelected}
		/>
	) : null;

	return (
		<div className="flex h-full min-h-0 flex-col gap-4">
			<div className="flex shrink-0 flex-wrap items-center justify-between gap-4">
				<div>
					<h1 className="text-2xl font-bold tracking-tight">Отделы</h1>

					<p className="text-muted-foreground text-sm">
						{view === "active"
							? "Действующая организационная структура"
							: "Удалённые отделы"}
					</p>
				</div>
				<DepartmentViewSwitch value={view} onValueChange={setView} />
			</div>

			<div className="min-h-0 flex-1 overflow-y-auto">
				{view === "active" ? (
					<DepartmentPositions
						positionsDepartmentId={positionsDepartmentId}
						focusedDepartment={focusedDepartment}
					/>
				) : (
					<div className="flex min-h-0 flex-col gap-4 pl-3">
						{focusedDepartment}
						<ArchivedDepartmentList />
					</div>
				)}
			</div>
		</div>
	);
}
