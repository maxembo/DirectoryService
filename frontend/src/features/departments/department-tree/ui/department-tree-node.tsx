"use client";

import { DepartmentTreeDto } from "@/entities/departments/model/types";
import { Button } from "@/shared/components/ui/button";
import { cn } from "@/shared/lib/utils";
import { ChevronRight, LoaderCircle } from "lucide-react";
import { DepartmentTreeId } from "../model/department-tree-store";
import { useDepartmentTreeNode } from "../model/use-department-tree-node";

const TREE_INDENT = 22;

type Props = {
	department: DepartmentTreeDto;
	depth: number;
	stateId?: DepartmentTreeId;
};

export function DepartmentTreeNode({ stateId, department, depth }: Props) {
	const {
		children,
		isSelected,
		isExpanded,
		hasChildren,
		isLoading,
		canLoadMore,
		handleSelect,
		handleToggle,
		handleLoadMore,
	} = useDepartmentTreeNode({ department, stateId });

	return (
		<li className="relative list-none">
			<div
				className={cn(
					"group relative flex min-h-10 items-center gap-1 rounded-md pr-2 transition-colors",
					isSelected
						? "bg-accent text-accent-foreground ring-1 ring-inset ring-border"
						: "hover:bg-muted/70",
				)}
				style={{
					paddingLeft: 8 + depth * TREE_INDENT,
				}}
			>
				<TreeGuideLines depth={depth} />

				<div className="relative z-10 flex size-6 shrink-0 items-center justify-center">
					{hasChildren ? (
						<button
							type="button"
							className={cn(
								"flex size-6 items-center justify-center rounded-sm text-muted-foreground",
								"hover:bg-background hover:text-foreground",
								"focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
							)}
							onClick={handleToggle}
						>
							{isLoading ? (
								<LoaderCircle className="size-4 animate-spin" />
							) : (
								<ChevronRight
									className={cn(
										"size-4 transition-transform duration-150",
										isExpanded && "rotate-90",
									)}
								/>
							)}
						</button>
					) : (
						<span
							className="size-1.5 rounded-full bg-muted-foreground/40"
							aria-hidden="true"
						/>
					)}
				</div>

				<button
					type="button"
					className={cn(
						"relative z-10 flex min-w-0 flex-1 items-center gap-3 py-2 text-left",
						"focus-visible:outline-none",
					)}
					onClick={handleSelect}
				>
					<span className="min-w-0 flex-1">
						<span
							className={cn(
								"block truncate text-sm",
								isSelected ? "font-semibold" : "font-medium",
							)}
						>
							{department.name}
						</span>
					</span>

					<span
						className={cn(
							"max-w-32 shrink-0 truncate rounded px-1.5 py-0.5",
							"font-mono text-[10px] leading-none",
							isSelected
								? "bg-background/70 text-foreground"
								: "bg-muted text-muted-foreground",
						)}
						title={department.identifier}
					>
						{department.identifier}
					</span>

					{!department.isActive && (
						<span
							className="size-2 shrink-0 rounded-full bg-amber-500"
							title="Неактивное подразделение"
						/>
					)}
				</button>
			</div>

			{isExpanded && (
				<ul className="space-y-0.5">
					{children.map((child) => (
						<DepartmentTreeNode
							key={child.id}
							department={child}
							stateId={stateId}
							depth={depth + 1}
						/>
					))}

					{isLoading && children.length === 0 && (
						<TreeNodeLoading depth={depth + 1} />
					)}
					{canLoadMore && (
						<li>
							<Button onClick={handleLoadMore}>
								{isLoading ? "Загрузка..." : "Показать ещё"}
							</Button>
						</li>
					)}
				</ul>
			)}
		</li>
	);
}

type TreeGuideLinesProps = {
	depth: number;
};

function TreeGuideLines({ depth }: TreeGuideLinesProps) {
	if (depth === 0) return null;

	return (
		<>
			{Array.from({ length: depth }).map((_, index) => (
				<span
					key={index}
					className="pointer-events-none absolute inset-y-0 w-px bg-blue-400"
					style={{
						left: 18 + index * TREE_INDENT,
					}}
					aria-hidden="true"
				/>
			))}
		</>
	);
}

type TreeNodeLoadingProps = {
	depth: number;
};

function TreeNodeLoading({ depth }: TreeNodeLoadingProps) {
	return (
		<li
			className="flex h-9 items-center gap-2 text-xs text-muted-foreground"
			style={{
				paddingLeft: 8 + depth * TREE_INDENT + 30,
			}}
		>
			<LoaderCircle className="size-3.5 animate-spin" />
			Загрузка…
		</li>
	);
}
