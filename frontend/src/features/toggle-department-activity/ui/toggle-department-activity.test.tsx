import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ToggleDepartmentActivity } from "./toggle-department-activity";

const mocks = vi.hoisted(() => ({
	toggleDepartmentActivity: vi.fn(),
	isPending: false,
}));

vi.mock("../model/use-toggle-department-activity", () => ({
	useToggleDepartmentActivity: () => ({
		toggleDepartmentActivity: mocks.toggleDepartmentActivity,
		isPending: mocks.isPending,
	}),
}));

const department = {
	id: "department-id",
	name: "Разработка",
	identifier: "development",
	parentId: null,
	isActive: true,
	depth: 0,
	hasChildren: false,
	path: "development",
};

describe("ToggleDepartmentActivity", () => {
	beforeEach(() => {
		vi.clearAllMocks();
		mocks.isPending = false;
	});

	it("sends the explicitly selected activity state", async () => {
		const user = userEvent.setup();
		render(<ToggleDepartmentActivity department={department} />);

		const activitySwitch = screen.getByRole("switch", { name: "Активно" });
		expect(activitySwitch).toBeChecked();

		await user.click(activitySwitch);

		expect(mocks.toggleDepartmentActivity).toHaveBeenCalledWith({
			departmentId: "department-id",
			isActive: false,
		});
	});

	it("shows pending state and disables repeated interaction", () => {
		mocks.isPending = true;

		render(<ToggleDepartmentActivity department={department} />);

		expect(screen.getByRole("switch", { name: "Сохранение" })).toBeDisabled();
	});
});
