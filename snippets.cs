-----
ExceptionInfo ei = new ExceptionInfo(new QAHabroGrabProgram().GetType().Name, null);

-----
/*
            PingResponse ping_response = new PingResponse(true);
            ping_response.error.Time = DateTime.Now.ToString("s");
            ping_response.error.Text = "Просто ошибка";
            ping_response.error.Exception.Message = "Ошипка, блин.";
            ping_response.error.Exception.ClassName = "QAHabroGrabProgram";
            log.Debug(String.Format("ping_response.Result = {0}", ping_response.Result));
            log.Debug(String.Format("Ошибка: {0}", ping_response.error.Text));
            */

-----
	    int requestId = 42;
            int version = 123;
            List<string> queries = new List<string>();
            queries.Add("Первый запрос");
            queries.Add("Второй запрос");
            Period prd = new Period(DateTime.Now.ToString("s"), DateTime.UtcNow.ToString("s"));
            GrabRequest gr1 = new GrabRequest(requestId, version, queries, prd);
            GrabRequest gr2 = new GrabRequest(requestId+1, version+3, queries, prd);
            GrabRequest gr3 = new GrabRequest(requestId+2, version+4, queries, prd);

-----
/*
            GrabResponse grab_resp = new GrabResponse(false);
            grab_resp.Error.Text = "Текст в ответе";
            grab_resp.Error.Time = DateTime.Now.ToString("s");
            grab_resp.Error.Exception.ClassName = "СуперПупер класс";
            grab_resp.Error.Exception.Message = "Ацкая мессага";
            grab_resp.Error.Exception.StackTrace.Add("Первый уровень стека");
            grab_resp.Error.Exception.StackTrace.Add("Второй уровень стека");
            grab_resp.Error.Exception.StackTrace.Add("Третий уровень стека");
            */

            /*
            List<AuthorInfo> ail = new List<AuthorInfo>();
            ail.Add(new AuthorInfo("Александр Ящук", "a.yashuk@pflb.ru"));
            SourceInfo si = new SourceInfo("Хабрахабр", "https://habrahabr.ru", logo_base64);
            GrabberInfo gi = new GrabberInfo("Первый грабер", "1.2.3", ail, si);
            PostInfo pi = new PostInfo(site_page, "ru", title: "Заголовок типа");
            ProcessingInfo proc_info = new ProcessingInfo(DateTime.UtcNow.ToString("s"), DateTime.Now.ToString("s"));
            GrabResults grab_result_1 = new GrabResults(search_query, pi, proc_info);
            List<GrabResults> grab_rezult_list = new List<GrabResults>();
            grab_rezult_list.Add(grab_result_1);
            GrabResultsRequest grrequ = new GrabResultsRequest(12, 15, gi, grab_rezult_list);
            */

            /*
            GrabResultsResponse grab_results_resp = new GrabResultsResponse(true);
            */



