"use client";

import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/shared/ui/list-empty";
import { ListError } from "@/shared/ui/list-error";
import { ChevronsDownUp } from "lucide-react";
import {
	collapseAllDepartments,
	DepartmentTreeId,
	useDepartmentTreeExpandedIds,
} from "../model/department-tree-store";
import { useDepartmentTreeRoots } from "../model/use-department-tree-roots";
import { DepartmentTreeNode } from "./department-tree-node";

type Props = {
	stateId?: DepartmentTreeId;
};

export function DepartmentTreeRootsList({ stateId }: Props) {
	const {
		departmentRoots,
		isPending,
		isError,
		error,
		cursorRef,
		isFetchingNextPage,
		isFetchNextPageError,
		fetchNextPage,
		refetch,
	} = useDepartmentTreeRoots({});

	const expandedIds = useDepartmentTreeExpandedIds(stateId);

	const hasExpandedBranches = expandedIds.length > 0;
	return (
		<section className="bg-background flex h-full min-h-0 w-full flex-col rounded-xl border">
			<div className="flex items-center justify-between border-b px-4 py-3">
				<div className="flex flex-col">
					<h1 className="text-lg font-semibold tracking-tight">Отделы</h1>
					<p className="text-muted-foreground text-sm">
						Выберите отдел из дерева
					</p>
				</div>

				<div>
					<Button
						type="button"
						variant="ghost"
						size="icon"
						title="Свернуть ветки"
						aria-label="Свернуть все ветки"
						disabled={!hasExpandedBranches}
						onClick={() => collapseAllDepartments(stateId)}
					>
						<ChevronsDownUp className="h-4 w-4" />
					</Button>
				</div>
			</div>

			{isPending ? (
				<div className="flex min-h-0 flex-1 items-center justify-center">
					<Spinner />
				</div>
			) : isError && departmentRoots.length === 0 ? (
				<div className="p-4">
					<ListError
						message={error?.message ?? "Неизвестная ошибка"}
						onRetry={refetch}
					/>
				</div>
			) : departmentRoots.length === 0 ? (
				<div className="p-4">
					<ListEmpty title="Подразделения" />
				</div>
			) : (
				<div className="min-h-0 flex-1 overflow-y-auto overscroll-contain p-2">
					<ul className="relative space-y-1">
						{departmentRoots.map((department) => (
							<DepartmentTreeNode
								key={department.id}
								stateId={stateId}
								department={department}
								depth={0}
							/>
						))}
					</ul>

					<div className="flex justify-center py-6">
						{isFetchNextPageError ? (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => void fetchNextPage()}
							>
								Повторить загрузку
							</Button>
						) : isFetchingNextPage ? (
							<Spinner />
						) : (
							<div ref={cursorRef} />
						)}
					</div>
				</div>
			)}
		</section>
	);
}
