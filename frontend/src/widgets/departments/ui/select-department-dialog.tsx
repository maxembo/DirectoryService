import { DepartmentShortDto } from "@/entities/departments/model/types";
import { Button } from "@/shared/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/shared/components/ui/dialog";
import { InfinityDepartmentsList } from "./infinity-departments-list";

type Props = {
	selectedDepartments: DepartmentShortDto[];
	onChange: (selectedDepartments: DepartmentShortDto[]) => void;
	multiSelect?: boolean;
	excludeIds?: string[];
	open: boolean;
	setOpen: (open: boolean) => void;
};

export function SelectDepartmentDialog({
	selectedDepartments,
	onChange,
	multiSelect = false,
	excludeIds = [],
	open,
	setOpen,
}: Props) {
	return (
		<Dialog open={open} onOpenChange={setOpen}>
			<DialogTrigger asChild>
				<Button variant="outline">Выбрать подразделение</Button>
			</DialogTrigger>

			<DialogContent className="flex h-[75dvh] flex-col sm:max-w-2xl">
				<DialogHeader>
					<DialogTitle>Выбрать подразделение</DialogTitle>
					<DialogDescription>
						Найдите подразделение и выберите его из списка
					</DialogDescription>
				</DialogHeader>

				<div className="flex-1 min-h-0">
					<InfinityDepartmentsList
						stateId="multi-select-locations"
						selectedDepartments={selectedDepartments}
						onChange={onChange}
						multiSelect={multiSelect}
						excludeIds={excludeIds}
					/>
				</div>
			</DialogContent>
		</Dialog>
	);
}
