import { useDepartment } from "@/entities/departments";
import { FocusedCard } from "@/shared/ui/focused-card";
import type { ArchiveView } from "@/shared/hooks";
import { ListEmpty } from "@/shared/ui/list-empty";
import { DepartmentDetailCard } from "./department-detail-card";
import { ArchivedDepartmentCard } from "../archive/archived-department-card";
import {
	setDepartmentTreeSelectedId,
	type DepartmentTreeId,
} from "@/features/department-tree";

type Props = {
	id: string;
	view: ArchiveView;
	onRemove: () => void;
	stateId?: DepartmentTreeId;
	isSelected: boolean;
};

export function FocusedDepartment({
	id,
	view,
	onRemove,
	stateId,
	isSelected,
}: Props) {
	const { department, isLoading, isError, error, refetch } = useDepartment(id);

	return (
		<FocusedCard
			onRemove={onRemove}
			isLoading={isLoading}
			isError={isError}
			error={error}
			refetch={refetch}
			isSelected={isSelected}
		>
			{department ? (
				view === "active" ? (
					<div className="items-center gap-4">
						<DepartmentDetailCard
							department={department}
							onSelect={() => setDepartmentTreeSelectedId(null, stateId)}
						/>
					</div>
				) : (
					department.deletedAt !== null && (
						<ArchivedDepartmentCard department={department} />
					)
				)
			) : (
				<ListEmpty title={"Ничего не найдено"} />
			)}
		</FocusedCard>
	);
}
