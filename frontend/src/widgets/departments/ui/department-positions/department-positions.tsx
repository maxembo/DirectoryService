import { DepartmentTreeRootsList } from "@/features/department-tree";
import { ToggleDepartmentActivity } from "@/features/toggle-department-activity";
import { DepartmentPositionsList } from "./department-positions-list";
import { MoveDepartmentAction } from "../move-department/move-department-action";
import type { DepartmentId } from "@/entities/departments";
import type { ReactNode } from "react";

type Props = {
	positionsDepartmentId: DepartmentId | null;
	focusedDepartment: ReactNode;
};

export function DepartmentPositions({
	positionsDepartmentId,
	focusedDepartment,
}: Props) {
	return (
		<div className="grid h-full min-h-0 grid-cols-2 gap-6">
			<div className="flex min-h-0 flex-col gap-4">
				{focusedDepartment && <div className="pl-0.5">{focusedDepartment}</div>}
				<div className="min-h-0 flex-1">
					<DepartmentTreeRootsList
						renderActions={(department) => (
							<div className="flex shrink-0 items-center gap-1 border-l pl-3">
								<ToggleDepartmentActivity department={department} />
								<MoveDepartmentAction department={department} />
							</div>
						)}
					/>
				</div>
			</div>

			<DepartmentPositionsList positionsDepartmentId={positionsDepartmentId} />
		</div>
	);
}
