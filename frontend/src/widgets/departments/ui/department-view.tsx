"use client";

import { ArchiveViewSwitch } from "@/shared/components/archive-view-switch";
import { useArchiveView } from "@/shared/hooks/use-archive-view";
import { ArchivedDepartmentList } from "./archived-department-list";
import { DepartmentPositions } from "@/widgets/department-positions";

export function DepartmentView() {
	const { view, setView } = useArchiveView();

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
				<ArchiveViewSwitch
					value={view}
					onValueChange={setView}
					title="Режим отображения отделов"
				/>
			</div>

			<div className="min-h-0 flex-1 overflow-y-auto">
				{view === "active" ? (
					<DepartmentPositions />
				) : (
					<ArchivedDepartmentList />
				)}
			</div>
		</div>
	);
}
