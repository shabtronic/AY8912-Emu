public class AY8912
{
    public long[] R;                                                                                                            // Regs and counters all in here if you need to serialize the enitre state easily
    public int RegisterSelect;
    private short[] V = { 0, 112, 167, 238, 346, 506, 693, 1121, 1385, 2168, 2888, 3685, 4672, 5629, 6947, 8191, 0 };
    private short[] RM = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 };
    private short[] MT = { 0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf };
    public int AYWrites;
 
    public AY8912()
    {
        Reset();
    }
 
    public void Reset()
    {
        R = new long[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xACE1, 0, 0, 0, 0, 0, 0, 0 };
    }
 
    public void PortWrite(int val)
    {
        if (RegisterSelect == 13)
        {
            R[26] = MT[val & 4];
            R[27] = 0x0f;
        }
        R[RegisterSelect & 0xf] = val & 0xff;
        AYWrites++;
    }
 
    public int PortRead() { return (int)(R[RegisterSelect] & 0xff); }
 
    public int Update(int ClockDiv)                                                                                                 // 1= 110.25khz 2=55khz e.t.c
    {
        R[25] += (R[25] >> 31 & ((R[11] | R[12] << 8) * 2)) - ClockDiv;                                                             // Update Envelope - not complete hold/continue non functional
        R[27] += ((R[27] >> 27) & 15) - (R[25] >> 63);
        R[26] = (R[26] ^ ((R[26] >> 27) & MT[R[13] & 2])) & 0x0f;
        V[16] = V[(R[27] ^ R[26]) & 0xf];
        R[20] += ((R[20] >> 19) & (((R[0] & 0xff) | R[1] << 8) & 0xfff)) - ClockDiv; R[16] ^= R[20] >> 63;                          // Update Audio channels
        R[21] += ((R[21] >> 19) & (((R[2] & 0xff) | R[3] << 8) & 0xfff)) - ClockDiv; R[17] ^= R[21] >> 63;
        R[22] += ((R[22] >> 19) & (((R[4] & 0xff) | R[5] << 8) & 0xfff)) - ClockDiv; R[18] ^= R[22] >> 63;
        R[23] += ((R[23] >> 19) & (R[6] & 0x1f)) - ClockDiv; R[19] ^= ((R[24] & 1) & (R[23] >> 31));                                // Update noise
        R[24] = ((R[24] >> 1) ^ (-(R[24] & 1) & 0xB400));
        int res = (ushort)((((((R[16] & 1) | ((R[7] >> 0) & 1)) & (R[19] | (R[7] >> 3))) & 1) - 1) & V[RM[R[8] & 31]]);             // Mix audio channels
        res += (short)((((((R[17] & 1) | ((R[7] >> 1) & 1)) & (R[19] | (R[7] >> 4))) & 1) - 1) & V[RM[R[9] & 31]]);
        res += (short)((((((R[18] & 1) | ((R[7] >> 2) & 1)) & (R[19] | (R[7] >> 5))) & 1) - 1) & V[RM[R[10] & 31]]);
        return res;
    }
}