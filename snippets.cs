-----
ExceptionInfo ei = new ExceptionInfo(new QAHabroGrabProgram().GetType().Name, null);

-----
/*
            PingResponse ping_response = new PingResponse(true);
            ping_response.error.Time = DateTime.Now.ToString("s");
            ping_response.error.Text = "������ ������";
            ping_response.error.Exception.Message = "������, ����.";
            ping_response.error.Exception.ClassName = "QAHabroGrabProgram";
            log.Debug(String.Format("ping_response.Result = {0}", ping_response.Result));
            log.Debug(String.Format("������: {0}", ping_response.error.Text));
            */

-----
	    int requestId = 42;
            int version = 123;
            List<string> queries = new List<string>();
            queries.Add("������ ������");
            queries.Add("������ ������");
            Period prd = new Period(DateTime.Now.ToString("s"), DateTime.UtcNow.ToString("s"));
            GrabRequest gr1 = new GrabRequest(requestId, version, queries, prd);
            GrabRequest gr2 = new GrabRequest(requestId+1, version+3, queries, prd);
            GrabRequest gr3 = new GrabRequest(requestId+2, version+4, queries, prd);

-----
/*
            GrabResponse grab_resp = new GrabResponse(false);
            grab_resp.Error.Text = "����� � ������";
            grab_resp.Error.Time = DateTime.Now.ToString("s");
            grab_resp.Error.Exception.ClassName = "���������� �����";
            grab_resp.Error.Exception.Message = "����� �������";
            grab_resp.Error.Exception.StackTrace.Add("������ ������� �����");
            grab_resp.Error.Exception.StackTrace.Add("������ ������� �����");
            grab_resp.Error.Exception.StackTrace.Add("������ ������� �����");
            */

            /*
            List<AuthorInfo> ail = new List<AuthorInfo>();
            ail.Add(new AuthorInfo("��������� ����", "a.yashuk@pflb.ru"));
            SourceInfo si = new SourceInfo("���������", "https://habrahabr.ru", logo_base64);
            GrabberInfo gi = new GrabberInfo("������ ������", "1.2.3", ail, si);
            PostInfo pi = new PostInfo(site_page, "ru", title: "��������� ����");
            ProcessingInfo proc_info = new ProcessingInfo(DateTime.UtcNow.ToString("s"), DateTime.Now.ToString("s"));
            GrabResults grab_result_1 = new GrabResults(search_query, pi, proc_info);
            List<GrabResults> grab_rezult_list = new List<GrabResults>();
            grab_rezult_list.Add(grab_result_1);
            GrabResultsRequest grrequ = new GrabResultsRequest(12, 15, gi, grab_rezult_list);
            */

            /*
            GrabResultsResponse grab_results_resp = new GrabResultsResponse(true);
            */



