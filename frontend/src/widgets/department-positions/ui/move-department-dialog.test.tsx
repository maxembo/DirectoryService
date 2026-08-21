import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ComponentProps } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MoveDepartmentDialog } from "./move-department-dialog";

const mocks = vi.hoisted(() => ({
	moveDepartment: vi.fn(),
	reset: vi.fn(),
	selectorProps: undefined as
		| {
				activeOnly?: boolean;
				excludeIds?: string[];
				excludeSubtreePath?: string;
				onChange: (departments: unknown[]) => void;
		  }
		| undefined,
}));

vi.mock("@/features/select-department", () => ({
	SelectDepartmentDialog: (props: {
		activeOnly?: boolean;
		excludeIds?: string[];
		excludeSubtreePath?: string;
		onChange: (departments: unknown[]) => void;
	}) => {
		mocks.selectorProps = props;

		return (
			<button
				type="button"
				onClick={() =>
					props.onChange([
						{
							id: "new-parent-id",
							name: "Новый родитель",
							identifier: "new-parent",
							path: "company.new-parent",
							isActive: true,
							createdAt: "2026-08-21T00:00:00Z",
							updatedAt: "2026-08-21T00:00:00Z",
							deletedAt: null,
						},
					])
				}
			>
				Выбрать подразделение
			</button>
		);
	},
	SelectedDepartment: () => null,
}));

vi.mock("@/features/move-department", () => ({
	useMoveDepartment: () => ({
		moveDepartment: mocks.moveDepartment,
		isPending: false,
		error: undefined,
		reset: mocks.reset,
	}),
}));

const department: ComponentProps<typeof MoveDepartmentDialog>["department"] = {
	id: "department-id",
	name: "Разработка",
	identifier: "dev",
	parentId: "current-parent-id",
	isActive: true,
	depth: 1,
	hasChildren: true,
	path: "company.dev",
};

describe("MoveDepartmentDialog", () => {
	beforeEach(() => {
		vi.clearAllMocks();
		mocks.selectorProps = undefined;
		mocks.moveDepartment.mockImplementation(
			(_variables, options?: { onSuccess?: () => void }) => {
				options?.onSuccess?.();
			},
		);
	});

	it("moves a department to the root and closes after success", async () => {
		const user = userEvent.setup();
		const onOpenChange = vi.fn();
		render(
			<MoveDepartmentDialog
				department={department}
				open
				onOpenChange={onOpenChange}
			/>,
		);

		await user.click(
			screen.getByRole("button", { name: "Перенести в корень" }),
		);
		await user.click(screen.getByRole("button", { name: "Перенести" }));

		expect(mocks.moveDepartment).toHaveBeenCalledWith(
			{ departmentId: "department-id", parentId: null },
			expect.objectContaining({ onSuccess: expect.any(Function) }),
		);
		expect(onOpenChange).toHaveBeenCalledWith(false);
	});

	it("offers only valid active parents and sends the selected parent", async () => {
		const user = userEvent.setup();
		render(
			<MoveDepartmentDialog
				department={department}
				open
				onOpenChange={vi.fn()}
			/>,
		);

		expect(mocks.selectorProps).toMatchObject({
			activeOnly: true,
			excludeIds: ["department-id", "current-parent-id"],
			excludeSubtreePath: "company.dev",
		});

		await user.click(
			screen.getByRole("button", { name: "Выбрать подразделение" }),
		);
		await user.click(screen.getByRole("button", { name: "Перенести" }));

		expect(mocks.moveDepartment).toHaveBeenCalledWith(
			{ departmentId: "department-id", parentId: "new-parent-id" },
			expect.objectContaining({ onSuccess: expect.any(Function) }),
		);
	});
});
