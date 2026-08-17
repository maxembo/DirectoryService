"use client";

import type { DepartmentTreeDto } from "@/entities/departments";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import type { DepartmentTreeId } from "../model/department-tree-store";
import { useDepartmentTreeNode } from "../model/use-department-tree-node";
import { DepartmentTreeNodeRow } from "./department-tree-node-row";
import {
	TREE_BASE_PADDING,
	TREE_CONTENT_OFFSET,
	TREE_INDENT,
} from "./department-tree.constants";

type Props = {
	department: DepartmentTreeDto;
	depth: number;
	stateId?: DepartmentTreeId;
};

export function DepartmentTreeNode({ stateId, department, depth }: Props) {
	const node = useDepartmentTreeNode({ department, stateId });

	return (
		<li className="relative list-none">
			<DepartmentTreeNodeRow
				department={department}
				isSelected={node.isSelected}
				depth={depth}
				isLoading={node.isLoading}
				isExpanded={node.isExpanded}
				hasChildren={node.hasChildren}
				onToggle={node.handleToggle}
				onSelect={node.handleSelect}
			/>

			{node.isExpanded && (
				<DepartmentTreeChildren
					departmentChildren={node.departmentChildren}
					stateId={stateId}
					depth={depth}
					isError={node.isError}
					errorMessage={node.errorMessage}
					hasNextPage={node.hasNextPage}
					isFetchingNextPage={node.isFetchingNextPage}
					isFetchNextPageError={node.isFetchNextPageError}
					onRetry={node.handleRetry}
					onLoadMore={node.handleLoadMore}
				/>
			)}
		</li>
	);
}

type DepartmentTreeChildrenProps = {
	departmentChildren: DepartmentTreeDto[];
	stateId?: DepartmentTreeId;
	depth: number;
	isError: boolean;
	errorMessage: string;
	hasNextPage: boolean;
	isFetchingNextPage: boolean;
	isFetchNextPageError: boolean;
	onRetry: () => void;
	onLoadMore: () => void;
};

function DepartmentTreeChildren({
	departmentChildren,
	stateId,
	depth,
	isError,
	errorMessage,
	hasNextPage,
	isFetchingNextPage,
	isFetchNextPageError,
	onRetry,
	onLoadMore,
}: DepartmentTreeChildrenProps) {
	return (
		<ul className="space-y-0.5">
			{departmentChildren.map((child) => (
				<DepartmentTreeNode
					key={child.id}
					department={child}
					stateId={stateId}
					depth={depth + 1}
				/>
			))}

			{isError && departmentChildren.length === 0 && (
				<TreeNodeError
					depth={depth + 1}
					message={errorMessage}
					onRetry={onRetry}
				/>
			)}
			{hasNextPage && departmentChildren.length > 0 && (
				<li
					style={{
						paddingLeft:
							TREE_BASE_PADDING +
							(depth + 1) * TREE_INDENT +
							TREE_CONTENT_OFFSET,
					}}
				>
					<Button
						type="button"
						variant="ghost"
						size="sm"
						onClick={onLoadMore}
						disabled={isFetchingNextPage}
					>
						{isFetchingNextPage ? (
							<Spinner />
						) : isFetchNextPageError ? (
							"Повторить загрузку"
						) : (
							"Показать ещё"
						)}
					</Button>
				</li>
			)}
		</ul>
	);
}

type TreeNodeErrorProps = {
	depth: number;
	message: string;
	onRetry: () => void;
};

function TreeNodeError({ depth, message, onRetry }: TreeNodeErrorProps) {
	return (
		<li
			className="text-destructive flex items-center gap-2 py-2 text-xs"
			style={{
				paddingLeft:
					TREE_BASE_PADDING + depth * TREE_INDENT + TREE_CONTENT_OFFSET,
			}}
		>
			<span>{message}</span>

			<Button type="button" variant="outline" size="sm" onClick={onRetry}>
				Повторить
			</Button>
		</li>
	);
}
