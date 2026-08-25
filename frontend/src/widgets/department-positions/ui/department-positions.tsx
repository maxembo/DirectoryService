import { DepartmentTreeRootsList } from "@/features/department-tree";
import { ToggleDepartmentActivity } from "@/features/toggle-department-activity";
import { DepartmentPositionsList } from "./department-positions-list";
import { MoveDepartmentAction } from "./move-department-action";

export function DepartmentPositions() {
	return (
		<div className="grid h-full min-h-0 grid-cols-2 gap-6">
			<DepartmentTreeRootsList
				renderActions={(department) => (
					<div className="flex shrink-0 items-center gap-1 border-l pl-3">
						<ToggleDepartmentActivity department={department} />
						<MoveDepartmentAction department={department} />
					</div>
				)}
			/>
			<DepartmentPositionsList />
		</div>
	);
}
