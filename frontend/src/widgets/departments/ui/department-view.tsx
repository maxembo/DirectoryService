"use client";

import { ArchiveViewSwitch } from "@/shared/components/archive-view-switch";
import { useArchiveView } from "@/shared/hooks/use-archive-view";
import { DepartmentPositions } from "@/widgets/department-positions/ui/department-positions";
import { ArchivedDepartmentList } from "./archived-department-list";

export function DepartmentView() {
	const { view, setView } = useArchiveView();

	return (
		<div className="space-y-4">
			<div className="flex flex-wrap items-center justify-between gap-4">
				<div>
					<h1 className="text-2xl font-bold tracking-tight">Отделы</h1>
					<p className="text-sm text-muted-foreground">
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

			{view === "active" ? <DepartmentPositions /> : <ArchivedDepartmentList />}
		</div>
	);
}
