import type {
	DepartmentShortDto,
	DepartmentTreeDto,
} from "@/entities/departments";
import { useMoveDepartment } from "@/features/move-department";
import {
	SelectDepartmentDialog,
	SelectedDepartment,
} from "@/features/select-department";
import { Button } from "@/shared/components/ui/button";
import {
	Card,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import {
	Dialog,
	DialogClose,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/shared/components/ui/dialog";
import { Field, FieldGroup } from "@/shared/components/ui/field";
import { Label } from "@/shared/components/ui/label";
import { Spinner } from "@/shared/components/ui/spinner";
import { MoveRight } from "lucide-react";
import { useState } from "react";

type Props = {
	department: DepartmentTreeDto;
	open: boolean;
	onOpenChange: (nextOpen: boolean) => void;
};

type MoveTarget =
	| { kind: "unset" }
	| { kind: "root" }
	| { kind: "department"; department: DepartmentShortDto };

function formatDepartmentPath(path: string) {
	return path.split(".").join(" / ");
}

export function MoveDepartmentDialog({
	department,
	open,
	onOpenChange,
}: Props) {
	const { moveDepartment, isPending, error, reset } = useMoveDepartment();
	const [target, setTarget] = useState<MoveTarget>({ kind: "unset" });
	const [selectorOpen, setSelectorOpen] = useState(false);

	const parentError = error?.errorsList.find(
		(item) => item.invalidField === "department.parentId",
	);
	const operationError = error?.errorsList.find(
		(item) => item.invalidField !== "department.parentId",
	);

	const selectedDepartments =
		target.kind === "department" ? [target.department] : [];
	const currentParentPath = department.parentId
		? formatDepartmentPath(department.path.split(".").slice(0, -1).join("."))
		: "Корневой уровень";
	const targetPath =
		target.kind === "root"
			? department.identifier
			: target.kind === "department"
				? `${formatDepartmentPath(target.department.path)} / ${department.identifier}`
				: null;

	const resetDialog = () => {
		setTarget({ kind: "unset" });
		setSelectorOpen(false);
		reset();
	};

	const handleOpenChange = (nextOpen: boolean) => {
		if (!nextOpen) resetDialog();
		onOpenChange(nextOpen);
	};

	const handleDepartmentChange = (departments: DepartmentShortDto[]) => {
		const newParent = departments[0];

		setTarget(
			newParent
				? { kind: "department", department: newParent }
				: { kind: "unset" },
		);
		setSelectorOpen(false);
		reset();
	};

	const handleSelectRoot = () => {
		setTarget({ kind: "root" });
		reset();
	};

	const handleRemoveTarget = () => {
		setTarget({ kind: "unset" });
		reset();
	};

	const handleMove = () => {
		if (target.kind === "unset") return;

		const parentId = target.kind === "root" ? null : target.department.id;

		moveDepartment(
			{ departmentId: department.id, parentId },
			{ onSuccess: () => handleOpenChange(false) },
		);
	};

	return (
		<Dialog open={open} onOpenChange={handleOpenChange}>
			<DialogContent className="sm:max-w-2xl">
				<DialogHeader>
					<DialogTitle>Перенести подразделение</DialogTitle>
					<DialogDescription>
						Выберите новое место в организационной структуре.
					</DialogDescription>
				</DialogHeader>

				<FieldGroup className="space-y-4">
					<div className="grid gap-4 sm:grid-cols-2">
						<Field>
							<Label>Подразделение</Label>
							<span className="wrap-break-word">{department.name}</span>
						</Field>
						<Field>
							<Label>Текущий родитель</Label>
							<span className="wrap-break-word">{currentParentPath}</span>
						</Field>
					</div>

					<Field data-invalid={Boolean(parentError)}>
						<Label>Новый родитель</Label>
						<SelectedDepartment
							selectedDepartments={selectedDepartments}
							onRemove={handleRemoveTarget}
						/>
						<div className="flex flex-wrap gap-2">
							<Button
								type="button"
								variant={target.kind === "root" ? "default" : "outline"}
								aria-pressed={target.kind === "root"}
								onClick={handleSelectRoot}
								disabled={department.parentId === null || isPending}
							>
								Перенести в корень
							</Button>

							<SelectDepartmentDialog
								selectedDepartments={selectedDepartments}
								onChange={handleDepartmentChange}
								activeOnly
								open={selectorOpen}
								setOpen={setSelectorOpen}
								excludeIds={[
									department.id,
									...(department.parentId ? [department.parentId] : []),
								]}
								excludeSubtreePath={department.path}
							/>
						</div>

						{parentError && (
							<p role="alert" className="text-destructive text-sm">
								{parentError.message}
							</p>
						)}

						<DialogDescription>
							Само подразделение, его текущий родитель и потомки исключены из
							списка.
						</DialogDescription>
					</Field>

					<Card className={targetPath ? "border-primary/40 bg-primary/5" : ""}>
						<CardHeader className="flex flex-col gap-3">
							<p
								className={
									targetPath ? "text-primary" : "text-muted-foreground"
								}
							>
								Предосмотр изменения
							</p>
							{targetPath ? (
								<div className="flex flex-col gap-5">
									<CardTitle className="text-muted-foreground grid grid-cols-[auto_1fr] gap-3">
										<span>Откуда</span>
										<span className="wrap-break-word">
											{formatDepartmentPath(department.path)}
										</span>
									</CardTitle>
									<CardTitle className="grid grid-cols-[auto_1fr] gap-3">
										<MoveRight className="text-primary" />
										<span className="text-muted-foreground grid grid-cols-[auto_1fr] gap-3">
											<span>Куда</span>
											<span className="text-primary wrap-break-word">
												{targetPath}
											</span>
										</span>
									</CardTitle>
									<CardDescription>
										Дочерние подразделения переместятся вместе с узлом.
									</CardDescription>
								</div>
							) : (
								<CardDescription>
									Выберите новое место, чтобы увидеть изменение.
								</CardDescription>
							)}
						</CardHeader>
					</Card>
				</FieldGroup>

				{operationError && (
					<p role="alert" className="text-destructive text-sm">
						{operationError.message}
					</p>
				)}

				<DialogFooter>
					<DialogClose asChild>
						<Button type="button" variant="outline" disabled={isPending}>
							Отмена
						</Button>
					</DialogClose>
					<Button
						type="button"
						disabled={target.kind === "unset" || isPending}
						onClick={handleMove}
					>
						{isPending && <Spinner />}
						{isPending ? "Перемещение..." : "Перенести"}
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
