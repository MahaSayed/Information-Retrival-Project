<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="final_IR_project.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="height: 234px">

    <img id="logo" src="FindslyLogo.jpg" alt="logo"/>

    <div class="main">
        <form id="form1" runat="server">
        <div>
            <!--<asp:Button ID="btnIndexing" runat="server" Text="indexing" OnClick="btnIndexing_Click" />-->
            <br />
        </div>
        
            <asp:TextBox ID="TextBox1" runat="server" Width="500px" Height="71px" ></asp:TextBox>
        <br /><br />
            <asp:RadioButton ID="RadioButton1" runat="server"  /> <label id="labelRadioButton1" ><b>Check Spelling</b></label>
            <!--<asp:RadioButton ID="RadioButton2" runat="server" /> <label><b>Soundax</b></label>-->
               <br /><br />
            <label><b>Did you mean ? </b></label>
               <asp:DropDownList ID="DropDownList1" runat="server" Width="200px" >
               
                </asp:DropDownList>
            <p style="height: 54px">
            <asp:Button ID="Button1" runat="server" Text="search" OnClick="Button1_Click" />
       
            </p>
            <p style="height: 168px">
                <asp:ListBox ID="ListBox1" runat="server" Width="1000px" Height="300px"  OnSelectedIndexChanged="ListBox1_SelectedIndexChanged"></asp:ListBox>
            </p>
       
        </form>
     </div>
</body>
</html>


<style>
    #TextBox1{
        transform:translateX(500px);
    }

    #RadioButton1{
        transform:translateX(500px);

    }
    #labelRadioButton1{
        margin-left: 500px;
         color: red;
    }

    #Button1{
        transform:translate(935px, -75px);
        font-family:'Copperplate Gothic';

    }
    body{
        background-color: #ffffff;
        font-family: 'Copperplate Gothic';
    }
    .main{
        transform:translateY(95px);
    }

    #logo {
        transform: translate(550px, 50px);
        height: 50px;
    }
    
</style>
