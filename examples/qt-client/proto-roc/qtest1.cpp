#include "qtest1.h"
#include "qtest1worker.h"
#include <QThread>
#include <QMap>
#include <QJSValue>
#include <QSharedPointer>
#include <QQmlEngine>
#include <QQmlContext>
using namespace Tests;
struct CallbackRequest
{
	QJSValue state;
	QJSValue callback;
};
class Tests::QTest1Private
{
public:
	QTest1Private() : currentRequestId(0) {}
	QThread workerThread;
	QTest1Worker* worker;
	QMap<int, QSharedPointer<CallbackRequest>> requests;
	int currentRequestId;
};
QTest1::QTest1(QObject* parent) : QObject(parent), d_priv(new Tests::QTest1Private())
{
	d_priv->worker = new QTest1Worker();
	d_priv->worker->moveToThread(&d_priv->workerThread);
	connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));
	d_priv->workerThread.start();
	connect(d_priv->worker, &QTest1Worker::testMethodNoRequestDone, this, &QTest1::testMethodNoRequestHandler);
}
QTest1::~QTest1()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
void QTest1::testMethodNoRequest(QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodNoRequest(requestId);
}
void QTest1::testMethodNoRequestHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
