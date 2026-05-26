export interface RunResult {
    output: string,
    success: boolean,
    modelJson?: string,
}
export class Elements {
    public testCode: string
    compile(code: string): Promise<RunResult>
    run(): Promise<RunResult>
    updateInputs(inputs: string): Promise<void>
}