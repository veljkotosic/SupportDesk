import {api} from "@/services/api";

const BASE_URL = `/api/v1/supportAgentInvite`

export const supportAgentInviteService = {
  async generateInviteCode(email: string): Promise<string> {
    const result = await api.post<{ code: string }>(BASE_URL, { email })
    return result.code
  },

  async revokeInviteCode(inviteId: string): Promise<void> {
    await api.delete<void>(`${BASE_URL}/${inviteId}`)
  }
}
