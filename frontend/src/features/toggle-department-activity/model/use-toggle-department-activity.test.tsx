import type { DepartmentShortDto } from "@/entities/departments";
import {
	EnvelopeError,
	type Envelope,
	type PaginationEnvelope,
} from "@/shared/api";
import {
	QueryClient,
	QueryClientProvider,
	type InfiniteData,
} from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import type { PropsWithChildren } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { useToggleDepartmentActivity } from "./use-toggle-department-activity";

const mocks = vi.hoisted(() => ({
	changeDepartmentActivity: vi.fn(),
	error: vi.fn(),
	success: vi.fn(),
}));

vi.mock("@/entities/departments", async (importOriginal) => {
	const original =
		await importOriginal<typeof import("@/entities/departments")>();

	return {
		...original,
		departmentsApi: {
			baseKey: "departments",
			changeDepartmentActivity: mocks.changeDepartmentActivity,
		},
	};
});

vi.mock("sonner", () => ({
	toast: {
		error: mocks.error,
		success: mocks.success,
	},
}));

type DepartmentData = InfiniteData<
	Envelope<PaginationEnvelope<DepartmentShortDto>>
>;

const department: DepartmentShortDto = {
	id: "department-id",
	name: "Разработка",
	identifier: "development",
	path: "development",
	isActive: true,
	createdAt: "2026-08-24T00:00:00Z",
	updatedAt: "2026-08-24T00:00:00Z",
	deletedAt: null,
};

function createData(): DepartmentData {
	return {
		pages: [
			{
				result: {
					items: [department],
					totalCount: 1,
					page: 1,
					pageSize: 20,
					totalPages: 1,
				},
				errorsList: [],
				isError: false,
				timeGenerated: "2026-08-24T00:00:00Z",
			},
		],
		pageParams: [1],
	};
}

function createWrapper(client: QueryClient) {
	return function Wrapper({ children }: PropsWithChildren) {
		return (
			<QueryClientProvider client={client}>{children}</QueryClientProvider>
		);
	};
}

describe("useToggleDepartmentActivity", () => {
	beforeEach(() => {
		vi.clearAllMocks();
	});

	it("optimistically updates the activity state before the request completes", async () => {
		const client = new QueryClient({
			defaultOptions: { mutations: { retry: false } },
		});
		const queryKey = ["departments", "list", { isActive: undefined }] as const;
		client.setQueryData(queryKey, createData());
		let resolveMutation!: (value: { result: string }) => void;
		mocks.changeDepartmentActivity.mockReturnValue(
			new Promise((resolve) => {
				resolveMutation = resolve;
			}),
		);

		const { result } = renderHook(() => useToggleDepartmentActivity(), {
			wrapper: createWrapper(client),
		});

		act(() => {
			result.current.toggleDepartmentActivity({
				departmentId: department.id,
				isActive: false,
			});
		});

		await waitFor(() => {
			const data = client.getQueryData<DepartmentData>(queryKey);
			expect(data?.pages[0].result?.items[0].isActive).toBe(false);
		});
		expect(result.current.isPending).toBe(true);

		act(() => resolveMutation({ result: department.id }));
		await waitFor(() => expect(result.current.isPending).toBe(false));
		expect(mocks.success).not.toHaveBeenCalled();
	});

	it("removes a deactivated department from only-active queries", async () => {
		const client = new QueryClient({
			defaultOptions: { mutations: { retry: false } },
		});
		const queryKey = [
			"departments",
			"tree-roots",
			{ onlyActive: true },
		] as const;
		client.setQueryData(queryKey, createData());
		mocks.changeDepartmentActivity.mockResolvedValue({ result: department.id });

		const { result } = renderHook(() => useToggleDepartmentActivity(), {
			wrapper: createWrapper(client),
		});

		act(() => {
			result.current.toggleDepartmentActivity({
				departmentId: department.id,
				isActive: false,
			});
		});

		await waitFor(() => {
			const data = client.getQueryData<DepartmentData>(queryKey);
			expect(data?.pages[0].result?.items).toEqual([]);
			expect(data?.pages[0].result?.totalCount).toBe(0);
		});
	});

	it("removes an activated department from inactive-only queries", async () => {
		const client = new QueryClient({
			defaultOptions: { mutations: { retry: false } },
		});
		const queryKey = ["departments", "list", { isActive: false }] as const;
		const data = createData();
		data.pages[0].result!.items[0] = { ...department, isActive: false };
		client.setQueryData(queryKey, data);
		mocks.changeDepartmentActivity.mockResolvedValue({ result: department.id });

		const { result } = renderHook(() => useToggleDepartmentActivity(), {
			wrapper: createWrapper(client),
		});

		act(() => {
			result.current.toggleDepartmentActivity({
				departmentId: department.id,
				isActive: true,
			});
		});

		await waitFor(() => {
			const updatedData = client.getQueryData<DepartmentData>(queryKey);
			expect(updatedData?.pages[0].result?.items).toEqual([]);
			expect(updatedData?.pages[0].result?.totalCount).toBe(0);
		});
	});

	it("rolls cache back and shows a backend business error", async () => {
		const client = new QueryClient({
			defaultOptions: { mutations: { retry: false } },
		});
		const queryKey = ["departments", "list", { isActive: undefined }] as const;
		client.setQueryData(queryKey, createData());
		let rejectMutation!: (reason: EnvelopeError) => void;
		mocks.changeDepartmentActivity.mockReturnValue(
			new Promise((_resolve, reject) => {
				rejectMutation = reject;
			}),
		);

		const { result } = renderHook(() => useToggleDepartmentActivity(), {
			wrapper: createWrapper(client),
		});

		act(() => {
			result.current.toggleDepartmentActivity({
				departmentId: department.id,
				isActive: false,
			});
		});

		await waitFor(() => {
			const data = client.getQueryData<DepartmentData>(queryKey);
			expect(data?.pages[0].result?.items[0].isActive).toBe(false);
		});

		act(() =>
			rejectMutation(
				new EnvelopeError([
					{
						code: "department.activity.active_descendants",
						message: "Сначала деактивируйте дочерние подразделения.",
						type: "conflict",
					},
				]),
			),
		);

		await waitFor(() => {
			const data = client.getQueryData<DepartmentData>(queryKey);
			expect(data?.pages[0].result?.items[0].isActive).toBe(true);
		});
		expect(mocks.error).toHaveBeenCalledWith(
			"Сначала деактивируйте дочерние подразделения.",
		);
	});
});
