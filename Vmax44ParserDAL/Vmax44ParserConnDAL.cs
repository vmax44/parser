using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Vmax44ParserConnectedLayer
{
    public class Order
    {   
        [Key]
        public int Id {get;set;}
        public string ClientName {get;set;}
        public string ClientCar {get;set;}
        public DateTime OrderDate {get;set;}
        public DateTime DTPDate {get;set;}
    }

    public class ParsedData
    {
        [Key]
        public int parseddataid { get; set; }
        public int orderid { get; set; }
        public string orig { get; set; }
        public string firmname { get; set; }
        public string searchedArtikul { get; set; }
        public string art { get; set; }
        public string desc { get; set; }
        public string statistic { get; set; }
        public decimal price { get; set; }
        public string parsertype { get; set; }
        public string url { get; set; }
        


        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(orig != null ? orig : "");
            s.Append(";").Append(parsertype != null ? parsertype : "");
            s.Append(";").Append(firmname != null ? firmname : "");
            s.Append(";").Append(art != null ? art : "");
            s.Append(";").Append(desc != null ? desc : "");
            s.Append(";").Append(statistic != null ? statistic : "");
            s.Append(";").Append(price.ToString());
            return s.ToString();
        }

        public ParsedData Copy()
        {
            ParsedData tmp = new ParsedData();
            tmp.parsertype = this.parsertype;
            tmp.orig = this.orig;
            tmp.firmname = this.firmname;
            tmp.art = this.art;
            tmp.desc = this.desc;
            tmp.statistic = this.statistic;
            tmp.price = this.price;
            return tmp;
        }
    }

    public class ParsedDataCollection : List<ParsedData>
    {
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            foreach (ParsedData d in this)
            {
                s.AppendLine(d.ToString());
            }
            return s.ToString();
        }
    }

    public class OrdersDAL : IDisposable
    {
        private SqlConnection sqlCn = null;

        public void OpenConnection(string ConnectionString)
        {
            sqlCn = new SqlConnection();
            sqlCn.ConnectionString = ConnectionString;
            sqlCn.Open();
        }

        public void CloseConnection()
        {
            sqlCn.Close();
        }

        public void InsertOrder(Order order)
        {
            string sql = string.Format("Insert Into Orders " +
                "(ClientName, ClientCar, OrderDate, DTPDate) Values " +
                "('{0}','{1}','{2}','{3}')", order.ClientName, order.ClientCar, order.OrderDate, order.DTPDate);
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public int LookUpOrderId(string ordernumber)
        {
            int orderid = 0;
            using (SqlCommand cmd=new SqlCommand("GetOrderIdByNumber",this.sqlCn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@OrderNumber";
                param.SqlDbType = SqlDbType.NChar;
                param.Value = ordernumber;
                param.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrderId";
                param.SqlDbType = SqlDbType.Int;
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                orderid=((int)cmd.Parameters["@OrderId"].Value);
            }
            return orderid;
        }

        public void InsertParsedData(int OrderId, ParsedData d)
        {
            string sql = string.Format("insert Into ParsedData " +
                "(OrderId, ParseDate, ParserType, Original, Firmname, Artikul, Description,Statistic,Price,"+
                "SearchedArtikul,Url) values " +
                "(@OrderId,@ParseDate,@ParserType,@Original,@Firmname,@Artikul,@Description,@Statistic,@Price,"+
                "@SearchedArtikul,@Url)");
            using (SqlCommand cmd=new SqlCommand(sql,this.sqlCn))
            {
                SqlParameter p = new SqlParameter();
                p.ParameterName = "@OrderId";
                p.Value = OrderId;
                p.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@SearchedArtikul";
                p.Value = d.searchedArtikul;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 20;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Url";
                p.Value = d.url;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 300;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@ParseDate";
                p.Value = DateTime.Now.Date;
                p.SqlDbType = SqlDbType.Date;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@ParserType";
                p.Value = d.parsertype;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 20;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Original";
                p.Value = d.orig;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 20;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Firmname";
                p.Value = d.firmname;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 20;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Artikul";
                p.Value = d.art;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 20;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Description";
                p.Value = d.desc;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 300;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Statistic";
                p.Value = d.statistic;
                p.SqlDbType = SqlDbType.NChar;
                p.Size = 10;
                cmd.Parameters.Add(p);

                p = new SqlParameter();
                p.ParameterName = "@Price";
                p.Value = d.price;
                p.SqlDbType = SqlDbType.Decimal;
                p.Precision = 8;
                p.Scale = 2;
                cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();
            }
        }

        public void InsertParsedDataCollection(int OrderId, ParsedDataCollection c)
        {
            foreach(ParsedData d in c)
            {
                this.InsertParsedData(OrderId, d);
            }
        }

        public void DeleteOrder(int id)
        {
            string sql = string.Format("Delete From Orders where Id='{0}'", id);
            using (SqlCommand cmd = new SqlCommand(sql, sqlCn))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(SqlException ex)
                {
                    Exception error = new Exception("Невозможно удалить отчет",ex);
                    throw error;
                }
            }
        }
        public DataTable GetAllOrdersAsDataTable()
        {
            DataTable ord = new DataTable();
            string sql = "Select * from Orders";
            using(SqlCommand cmd=new SqlCommand(sql,this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                ord.Load(dr);
                dr.Close();
            }
            return ord;
        }

        public Order GetOrder(int id)
        {
            Order order = new Order();
            string sql = string.Format("Select * from Orders where Id='{0}'", id);
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                if(dr.Read())
                {
                    order.Id = (int)dr["Id"];
                    order.ClientName = (string)dr["ClientName"];
                    order.ClientCar = (string)dr["ClientCar"];
                    order.OrderDate = (DateTime)dr["OrderDate"];
                    order.DTPDate = (DateTime)dr["DTPDate"];
                }
                dr.Close();
            }
            return order;
        }

        public void Dispose()
        {
            if (this.sqlCn != null) this.CloseConnection();
        }
    }
}
