import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import type { PropsWithChildren } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { useMoveDepartment } from "./use-move-department";

const mocks = vi.hoisted(() => ({
	invalidateQueries: vi.fn(),
	moveDepartment: vi.fn(),
	success: vi.fn(),
}));

vi.mock("@/entities/departments", () => ({
	departmentsApi: {
		baseKey: "departments",
		moveDepartment: mocks.moveDepartment,
	},
}));

vi.mock("@/shared/api", async (importOriginal) => {
	const original = await importOriginal<typeof import("@/shared/api")>();

	return {
		...original,
		queryClient: {
			invalidateQueries: mocks.invalidateQueries,
		},
	};
});

vi.mock("sonner", () => ({
	toast: {
		success: mocks.success,
		error: vi.fn(),
	},
}));

describe("useMoveDepartment", () => {
	beforeEach(() => {
		vi.clearAllMocks();
		mocks.moveDepartment.mockResolvedValue({ result: "department-id" });
		mocks.invalidateQueries.mockResolvedValue(undefined);
	});

	it("invalidates all department queries after a successful move", async () => {
		const client = new QueryClient({
			defaultOptions: { mutations: { retry: false } },
		});
		const wrapper = ({ children }: PropsWithChildren) => (
			<QueryClientProvider client={client}>{children}</QueryClientProvider>
		);
		const { result } = renderHook(() => useMoveDepartment(), { wrapper });

		act(() => {
			result.current.moveDepartment({
				departmentId: "department-id",
				parentId: "parent-id",
			});
		});

		await waitFor(() => {
			expect(mocks.invalidateQueries).toHaveBeenCalledWith({
				queryKey: ["departments"],
			});
		});
		expect(mocks.success).toHaveBeenCalledWith("Перенос подразделения успешно");
	});
});
