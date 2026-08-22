"use client";

import type { DepartmentTreeDto } from "@/entities/departments";
import { Button } from "@/shared/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuGroup,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { EllipsisVertical, MoveRight } from "lucide-react";
import { useState } from "react";
import { MoveDepartmentDialog } from "./move-department-dialog";

type Props = {
	department: DepartmentTreeDto;
};

export function MoveDepartmentAction({ department }: Props) {
	const [open, setOpen] = useState(false);

	return (
		<>
			<DropdownMenu>
				<DropdownMenuTrigger asChild>
					<Button
						type="button"
						variant="ghost"
						size="icon"
						aria-label={`Действия подразделения ${department.name}`}
					>
						<EllipsisVertical />
					</Button>
				</DropdownMenuTrigger>
				<DropdownMenuContent side="right" className="w-auto">
					<DropdownMenuGroup>
						<DropdownMenuItem onSelect={() => setOpen(true)}>
							<MoveRight />
							Перенести
						</DropdownMenuItem>
					</DropdownMenuGroup>
				</DropdownMenuContent>
			</DropdownMenu>

			<MoveDepartmentDialog
				open={open}
				onOpenChange={setOpen}
				department={department}
			/>
		</>
	);
}
