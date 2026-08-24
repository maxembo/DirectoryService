import { TooltipProvider } from "@/shared/components/ui/tooltip";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { MoveDepartmentAction } from "./move-department-action";

vi.mock("./move-department-dialog", () => ({
	MoveDepartmentDialog: () => null,
}));

const department = {
	id: "department-id",
	name: "Разработка",
	identifier: "development",
	parentId: null,
	isActive: false,
	depth: 0,
	hasChildren: false,
	path: "development",
};

describe("MoveDepartmentAction", () => {
	it("disables move actions for an inactive department", () => {
		render(
			<TooltipProvider>
				<MoveDepartmentAction department={department} />
			</TooltipProvider>,
		);

		expect(
			screen.getByRole("button", {
				name: "Перенос подразделения Разработка недоступен: сначала активируйте подразделение",
			}),
		).toBeDisabled();
	});
});
