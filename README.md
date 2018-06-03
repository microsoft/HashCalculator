# HashCalculator
Helper tool to calculate the value of TPM PCRs and hashes of data

This project allows to calculate of the value of TPM PCRs and of hashes of arbitrary data. It also contains functionality
to permutate hashes to determine the possible sequence of extend operations to arrive at a specified value. Since the number
of permutations that have to be tried is determined by the factorial of the number of hashes, this may take some time.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact 
[opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
