using System;
using System.Collections.Generic;
using UIWidgetsApp.Component.PullToRefresh;
using Unity.UIWidgets.async;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Screen
{
    public class RefreshListScreen : StatefulWidget
    {
        public override State createState()
        {
            return new RefreshListScreenState();
        }
    }

    internal class RefreshListScreenState : State<RefreshListScreen>
    {
        RefreshController _refreshController;
        private List<Widget> cards;

        public override void initState()
        {
            base.initState();
            _refreshController = new RefreshController();
            cards = new List<Widget>();
            GetCards();
        }

        private static Widget CreateCard(int? i = null)
        {
            var card = new Card(
                margin: EdgeInsets.symmetric(5, 10),
                child: new Center(child: new Text($"Card {i}"))
            );
            return card;
        }

        private void GetCards()
        {
            for (var i = 0; i < 14; i++)
            {
                cards.Add(CreateCard(i));
            }
        }

        private Widget BuildListView()
        {
            return new SmartRefresher(
                enablePullDown: true,
                enablePullUp: true,
                controller: _refreshController,
                onRefresh: up =>
                {
                    Future.delayed(TimeSpan.FromMilliseconds(1500)).then(val =>
                    {
                        cards.Add(CreateCard());
                        _refreshController.sendBack(up, up ? RefreshStatus.completed : RefreshStatus.idle);
                        setState(() => { });
                    });
                },
                child: ListView.builder(
                    itemExtent: 100,
                    itemCount: cards.Count,
                    itemBuilder: (context, index) => CreateCard(index)
                ));
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                body: new Container(
                    child: BuildListView()
                )
            );
        }
    }
}