import { DepartmentShortDto } from "@/entities/departments/model/types";
import { Badge } from "@/shared/components/ui/badge";
import {
	Card,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { CircleCheckBig, CircleX } from "lucide-react";

type Props = {
	department: DepartmentShortDto;
	onCheckedChange: (selected: boolean, department: DepartmentShortDto) => void;
	checked: boolean;
};

export function SelectDepartmentCard({
	department,
	checked,
	onCheckedChange,
}: Props) {
	const handleCheckedChange = (checked: boolean) => {
		onCheckedChange(checked, department);
	};
	return (
		<Card className="transition-colors hover:bg-muted/50">
			<CardHeader className="flex items-center p-4 gap-4">
				<Checkbox
					className="shrink-0"
					checked={checked}
					onCheckedChange={handleCheckedChange}
				/>

				<CardTitle className="min-w-0 flex-1 text-sm font-medium leading-5">
					{department.name}
				</CardTitle>

				<Badge
					variant={department.isActive ? "default" : "secondary"}
					className="shrink-0 gap-1 whitespace-nowrap"
				>
					{department.isActive ? (
						<>
							<CircleCheckBig className="h-3.5 w-3.5" />
							Активна
						</>
					) : (
						<>
							<CircleX className="h-3.5 w-3.5" />
							Неактивна
						</>
					)}
				</Badge>
			</CardHeader>
			<CardDescription className="px-5">
				{department.identifier}
			</CardDescription>
		</Card>
	);
}
