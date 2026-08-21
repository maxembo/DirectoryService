import type { DepartmentShortDto } from "@/entities/departments";
import { describe, expect, it } from "vitest";
import { isDepartmentExcluded } from "./select-department-list";

function createDepartment(id: string, path: string): DepartmentShortDto {
	return {
		id,
		path,
		name: id,
		identifier: id,
		isActive: true,
		createdAt: "2026-08-21T00:00:00Z",
		updatedAt: "2026-08-21T00:00:00Z",
		deletedAt: null,
	};
}

describe("isDepartmentExcluded", () => {
	it("excludes explicitly blocked departments", () => {
		const department = createDepartment("current-parent", "company");

		expect(isDepartmentExcluded(department, ["current-parent"])).toBe(true);
	});

	it("excludes the moved subtree but keeps similarly prefixed siblings", () => {
		const moved = createDepartment("dev", "company.dev");
		const descendant = createDepartment("frontend", "company.dev.frontend");
		const sibling = createDepartment("devops", "company.devops");

		expect(isDepartmentExcluded(moved, [], "company.dev")).toBe(true);
		expect(isDepartmentExcluded(descendant, [], "company.dev")).toBe(true);
		expect(isDepartmentExcluded(sibling, [], "company.dev")).toBe(false);
	});

	it("does not filter by path when a subtree was not specified", () => {
		const department = createDepartment("undefined-path", "undefined.child");

		expect(isDepartmentExcluded(department, [])).toBe(false);
	});
});
