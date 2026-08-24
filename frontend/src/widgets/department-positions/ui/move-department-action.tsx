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
import {
	Tooltip,
	TooltipContent,
	TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { EllipsisVertical, MoveRight } from "lucide-react";
import { useState } from "react";
import { MoveDepartmentDialog } from "./move-department-dialog";

type Props = {
	department: DepartmentTreeDto;
};

export function MoveDepartmentAction({ department }: Props) {
	const [open, setOpen] = useState(false);
	const canMove = department.isActive;

	const menuTrigger = (
		<DropdownMenuTrigger asChild>
			<Button
				type="button"
				variant="ghost"
				size="icon"
				disabled={!canMove}
				aria-label={
					canMove
						? `Действия подразделения ${department.name}`
						: `Перенос подразделения ${department.name} недоступен: сначала активируйте подразделение`
				}
			>
				<EllipsisVertical />
			</Button>
		</DropdownMenuTrigger>
	);

	return (
		<>
			<DropdownMenu>
				{canMove ? (
					menuTrigger
				) : (
					<Tooltip>
						<TooltipTrigger asChild>
							<span className="inline-flex" tabIndex={0}>
								{menuTrigger}
							</span>
						</TooltipTrigger>
						<TooltipContent side="top">
							Сначала активируйте подразделение
						</TooltipContent>
					</Tooltip>
				)}
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
