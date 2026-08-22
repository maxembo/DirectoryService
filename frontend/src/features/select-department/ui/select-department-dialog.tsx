import { Button } from "@/shared/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/shared/components/ui/dialog";
import type {
	DepartmentListId,
	DepartmentShortDto,
} from "@/entities/departments";
import { SelectDepartmentList } from "./select-department-list";

type Props = {
	selectedDepartments: DepartmentShortDto[];
	onChange: (selectedDepartments: DepartmentShortDto[]) => void;
	multiSelect?: boolean;
	excludeIds?: string[];
	excludeSubtreePath?: string;
	activeOnly?: boolean;
	stateId?: DepartmentListId;
	open: boolean;
	setOpen: (open: boolean) => void;
};

export function SelectDepartmentDialog({
	selectedDepartments,
	onChange,
	multiSelect = false,
	excludeIds = [],
	excludeSubtreePath,
	activeOnly = false,
	stateId,
	open,
	setOpen,
}: Props) {
	return (
		<Dialog open={open} onOpenChange={setOpen}>
			<DialogTrigger asChild>
				<Button type="button" variant="outline">
					Выбрать подразделение
				</Button>
			</DialogTrigger>

			<DialogContent className="flex h-[75dvh] flex-col sm:max-w-2xl">
				<DialogHeader>
					<DialogTitle>Выбрать подразделение</DialogTitle>
					<DialogDescription>
						Найдите подразделение и выберите его из списка
					</DialogDescription>
				</DialogHeader>

				<div className="min-h-0 flex-1">
					<SelectDepartmentList
						stateId={stateId}
						selectedDepartments={selectedDepartments}
						onChange={onChange}
						multiSelect={multiSelect}
						excludeIds={excludeIds}
						excludeSubtreePath={excludeSubtreePath}
						activeOnly={activeOnly}
					/>
				</div>
			</DialogContent>
		</Dialog>
	);
}
