namespace StockCopilot.Abstractions.Models;

// switch (market) {
//     case "1":
//     case "47": //东财指数
//     case "0":
//     case "2":
//         js = "aindex";
//         aindex()
//         break;
//     case "90":
//         js = "bk";
//         bk()
//         break;
//     case "116":
//         js = "hk";
//         hkscripts()
//         break;
//     case "105":
//     case "106":
//     case "107":
//     case "153":
//         js = "us";
//         us()
//         break;
// }

public record Stock(string Code, string Name)
{
    public override string ToString() => $"{Name} ({Code})";
}